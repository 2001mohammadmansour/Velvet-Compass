// CHANGED BY AI (2026-07-15): new page — sits between the room list (Rooms.js/SearchResults.js)
// and checkout (Reservation.js). Shows the full room listing (photo gallery, description, hotel
// amenities, then room amenities) with a sticky booking card on the right where dates/guests/
// add-ons are chosen and the running total is shown live. "Book Now" carries the resolved
// selection forward to Reservation.js, which is now a clean checkout-only page.
import { useEffect, useMemo, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import "./reservation.css";
import "./roomDetail.css";
import * as ownerSvc from "./services/owner";
import { getRoomTypeDetail, getHotelById } from "./services/hotels";
import { getPriceQuote } from "./services/pricing";
import { getRoomReviews } from "./services/guest";
import { getCurrentUser } from "./services/auth";

const CAT_LABELS = { staff: 'Staff', location: 'Location', facilities: 'Facilities', cleanliness: 'Cleanliness', comfort: 'Comfort', value: 'Value' };

function ReviewSnippet({ hotelId, roomId }) {
  const [data, setData] = useState(null);
  useEffect(() => {
    if (!roomId || !hotelId) return;
    getRoomReviews(hotelId, roomId).then(setData).catch(() => {});
  }, [hotelId, roomId]);
  if (!data || data.reviewCount === 0) return null;
  return (
    <div className="section-card">
      <h2 className="section-title" style={{ fontSize: '1.1rem', marginBottom: 12 }}>Guest Reviews</h2>
      <p style={{ marginBottom: 12 }}>
        <span style={{ fontSize: 28, fontWeight: 800, color: '#2a3d66' }}>{data.avgScore}</span>
        <span style={{ fontSize: 14, color: '#6b7280', marginLeft: 6 }}>/ 10 · {data.reviewCount} review{data.reviewCount !== 1 ? 's' : ''}</span>
      </p>
      {data.categoryAverages && (
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, marginBottom: 14 }}>
          {Object.entries(CAT_LABELS).map(([key, label]) => (
            <div key={key} style={{ background: '#f3f4f6', borderRadius: 8, padding: '5px 11px', textAlign: 'center', minWidth: 80 }}>
              <div style={{ fontSize: 11, color: '#6b7280', marginBottom: 2 }}>{label}</div>
              <div style={{ fontSize: 14, fontWeight: 700, color: '#1a2340' }}>{data.categoryAverages[key]}</div>
            </div>
          ))}
        </div>
      )}
      {data.reviews.slice(0, 3).map((r) => (
        <div key={r.id} style={{ borderTop: '1px solid #f3f4f6', paddingTop: 10, marginTop: 10 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
            <span style={{ fontWeight: 600, fontSize: 14, flex: 1 }}>{r.guestName}</span>
            <span style={{ background: '#2a3d66', color: '#fff', borderRadius: 6, padding: '2px 8px', fontSize: 13, fontWeight: 700 }}>{r.overallScore}/10</span>
            <span style={{ fontSize: 12, color: '#9ca3af' }}>{new Date(r.createdAt).toLocaleDateString()}</span>
          </div>
          <p style={{ margin: 0, fontSize: 14, color: '#374151', lineHeight: 1.5 }}>{r.comment}</p>
        </div>
      ))}
    </div>
  );
}

export default function RoomDetail() {
  const location = useLocation();
  const navigate = useNavigate();
  const incoming = location.state || {};
  const roomRef = incoming.room;
  const currentUser = getCurrentUser();

  const [room, setRoom] = useState(null);
  const [hotel, setHotel] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeImage, setActiveImage] = useState(0);

  const [dates, setDates] = useState({ checkIn: incoming.checkIn || '', checkOut: incoming.checkOut || '' });
  const [guests, setGuests] = useState(incoming.guests || 1);
  const [breakfast, setBreakfast] = useState(false);
  const [breakfastSettings, setBreakfastSettings] = useState(null);
  const [bookError, setBookError] = useState('');
  // CHANGED BY AI (2026-07-15): please review. New: the real server-computed price (seasonal +
  // occupancy surge) for the chosen dates, replacing the flat room.price * nights client-side
  // multiplication — this is the one piece of pricing math worth fetching rather than mirroring in
  // JS, since rule/occupancy data only exists server-side.
  const [quote, setQuote] = useState(null);
  const [quoteLoading, setQuoteLoading] = useState(false);

  useEffect(() => {
    if (!roomRef?.hotelId || !roomRef?.id) navigate('/hotels', { replace: true });
  }, [roomRef, navigate]);

  useEffect(() => {
    let mounted = true;
    if (!roomRef?.hotelId || !roomRef?.id) return;
    setLoading(true);
    setError('');
    Promise.all([
      getRoomTypeDetail(roomRef.hotelId, roomRef.id),
      getHotelById(roomRef.hotelId),
      ownerSvc.getSettings(roomRef.hotelId).catch(() => null),
    ])
      .then(([roomData, hotelData, settings]) => {
        if (!mounted) return;
        setRoom(roomData);
        setHotel(hotelData);
        if (settings?.breakfast?.available) setBreakfastSettings({ price: Number(settings.breakfast.price) || 0 });
      })
      .catch((err) => { if (mounted) setError(err.message || 'Unable to load room.'); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [roomRef?.hotelId, roomRef?.id]);

  const nights = useMemo(() => {
    if (!dates.checkIn || !dates.checkOut) return 0;
    const diff = new Date(dates.checkOut) - new Date(dates.checkIn);
    return Math.max(0, Math.floor(diff / (1000 * 60 * 60 * 24)));
  }, [dates.checkIn, dates.checkOut]);

  useEffect(() => {
    let mounted = true;
    if (!room || nights <= 0) { setQuote(null); return; }
    setQuoteLoading(true);
    getPriceQuote(room.hotelId, room.id, dates.checkIn, dates.checkOut)
      .then((q) => { if (mounted) setQuote(q); })
      .catch(() => { if (mounted) setQuote(null); })
      .finally(() => { if (mounted) setQuoteLoading(false); });
    return () => { mounted = false; };
  }, [room, dates.checkIn, dates.checkOut, nights]);

  // While the quote is loading (or if it ever fails), fall back to a flat estimate so the guest
  // always sees a reasonable number — the quote is what's shown once it resolves, and either way
  // the actual booking creation is authoritative.
  const quoteReasons = useMemo(() => {
    if (!quote) return [];
    return [...new Set(quote.nights.map((n) => n.reason).filter(Boolean))];
  }, [quote]);

  // CHANGED BY AI (2026-07-15): please review — extra beds are no longer a manual choice. Guests
  // just say how many people are staying, and if that's more than the room's base capacity, the
  // needed extra beds (up to the room's max) are chosen automatically. The guest only sees this
  // as an informational note + the resulting price, not a control.
  const maxExtraBeds = room?.allowExtraBed ? room.maxExtraBeds : 0;
  const hardCapacity = (room?.capacity || 1) + maxExtraBeds;
  const extraBedsNeeded = room ? Math.min(Math.max(0, guests - room.capacity), maxExtraBeds) : 0;

  const roomTotal = quote ? quote.total : (room?.price || 0) * nights;
  const breakfastTotal = breakfast && breakfastSettings ? breakfastSettings.price * nights * (guests || 1) : 0;
  const extraBedFeePerNight = extraBedsNeeded === 0 || !room
    ? 0
    : room.extraBedPriceType === 'fixed'
      ? (extraBedsNeeded === 1 ? Number(room.extraBedPriceForOneBed) || 0 : Number(room.extraBedPriceForTwoBeds) || 0)
      : (room.price * (extraBedsNeeded === 1 ? Number(room.extraBedPriceForOneBed) || 0 : Number(room.extraBedPriceForTwoBeds) || 0)) / 100;
  const extraBedTotal = extraBedFeePerNight * nights;
  const grandTotal = roomTotal + breakfastTotal + extraBedTotal;

  function handleBookNow() {
    setBookError('');
    if (!dates.checkIn || !dates.checkOut) { setBookError('Please choose check-in and check-out dates.'); return; }
    if (new Date(dates.checkIn) >= new Date(dates.checkOut)) { setBookError('Check-out date must be after check-in date.'); return; }
    if (guests > hardCapacity) {
      setBookError(`This room sleeps up to ${hardCapacity} guests${maxExtraBeds ? ` (including ${maxExtraBeds} extra bed${maxExtraBeds === 1 ? '' : 's'})` : ''}.`);
      return;
    }

    // CHANGED BY AI (2026-07-15): please review — passing the already-computed totals forward
    // (rather than just the raw selections) so Reservation.js doesn't need to re-derive the
    // extra-bed/breakfast pricing formulas a third time; it just displays these and submits them.
    navigate('/reservation', {
      state: {
        room: {
          id: room.id, hotelId: room.hotelId, name: room.name,
          hotel: hotel?.hotelName || '', city: hotel?.city || '',
          price: room.price, rating: hotel?.stars || 0,
          img: room.images?.[0]?.url,
          capacity: room.capacity,
        },
        checkIn: dates.checkIn, checkOut: dates.checkOut, guests, nights,
        breakfast, breakfastPricePerNight: breakfast && breakfastSettings ? breakfastSettings.price : 0, breakfastTotal,
        extraBeds: extraBedsNeeded, extraBedTotal,
        roomTotal, grandTotal,
      },
    });
  }

  if (!roomRef?.hotelId) {
    return (
      <div className="reservation-page">
        <div className="section-card"><p>No room selected. <Link to="/hotels">Browse hotels</Link></p></div>
      </div>
    );
  }

  return (
    <div className="rd-page">
      <div className="back-wrapper rd-back-wrapper">
        <button type="button" className="back-btn" onClick={() => navigate(-1)}>← Back</button>
      </div>

      {loading && <p className="sr-status">Loading room...</p>}
      {error && <p className="sr-status sr-error">{error}</p>}

      {room && (
        <div className="rd-layout">
          <main className="rd-main">
            {/* Photos */}
            <div className="section-card">
              {room.images.length > 0 ? (
                <>
                  <img className="rd-gallery-main" src={room.images[activeImage]?.url} alt={room.name} />
                  {room.images.length > 1 && (
                    <div className="rd-gallery-thumbs">
                      {room.images.map((img, i) => (
                        <img
                          key={img.id}
                          src={img.url}
                          alt=""
                          className={`rd-gallery-thumb${i === activeImage ? ' active' : ''}`}
                          onClick={() => setActiveImage(i)}
                        />
                      ))}
                    </div>
                  )}
                </>
              ) : (
                <div className="rd-gallery-placeholder">No photos yet</div>
              )}
            </div>

            {/* Description */}
            <div className="section-card">
              <h1 className="rd-room-name">{room.name}</h1>
              <p className="hotel">{hotel?.hotelName}</p>
              <p className="city">{hotel?.city}{hotel?.stars ? `  ·  ${'★'.repeat(hotel.stars)}` : ''}</p>
              {room.description && <p style={{ color: '#4b5563', marginTop: 10 }}>{room.description}</p>}
            </div>

            {/* Hotel amenities */}
            {Array.isArray(hotel?.amenities) && hotel.amenities.length > 0 && (
              <div className="section-card">
                <h2 className="section-title">Hotel Amenities</h2>
                <div className="rd-chip-row">
                  {hotel.amenities.map((a) => (
                    <span key={a.id} className="rd-chip">{a.icon ? `${a.icon} ` : ''}{a.name}</span>
                  ))}
                </div>
              </div>
            )}

            {/* Room amenities */}
            {room.amenities.length > 0 && (
              <div className="section-card">
                <h2 className="section-title">Room Amenities</h2>
                <div className="rd-chip-row">
                  {room.amenities.map((a) => (
                    <span key={a.id} className="rd-chip">{a.icon ? `${a.icon} ` : ''}{a.name}</span>
                  ))}
                </div>
              </div>
            )}

            <ReviewSnippet hotelId={room.hotelId} roomId={room.id} />
          </main>

          <aside className="rd-sidebar">
            <div className="section-card rd-booking-card">
              <h2 className="section-title">Book This Room</h2>
              <p className="price">${room.price}<span style={{ fontSize: 13, fontWeight: 500, color: '#6b7280' }}> / night</span></p>

              <label className="field-label">Check-in</label>
              <input type="date" value={dates.checkIn} onChange={(e) => setDates((d) => ({ ...d, checkIn: e.target.value }))} />

              <label className="field-label">Check-out</label>
              <input type="date" min={dates.checkIn || undefined} value={dates.checkOut} onChange={(e) => setDates((d) => ({ ...d, checkOut: e.target.value }))} />

              <label className="field-label">Guests</label>
              <input
                type="number"
                min={1}
                max={hardCapacity}
                value={guests}
                onChange={(e) => setGuests(Number(e.target.value) || 1)}
              />
              {/* CHANGED BY AI (2026-07-15): please review — extra beds are no longer a manual
                  dropdown; they're decided automatically from the guest count and shown here as an
                  informational note, matching the explicit request that the guest shouldn't have
                  to choose this themselves. */}
              {extraBedsNeeded > 0 ? (
                <p className="muted small" style={{ margin: '-8px 0 12px' }}>
                  This room includes {extraBedsNeeded} extra bed{extraBedsNeeded === 1 ? '' : 's'} to fit {guests} guests.
                </p>
              ) : maxExtraBeds > 0 ? (
                <p className="muted small" style={{ margin: '-8px 0 12px' }}>
                  Sleeps {room.capacity} without an extra bed, up to {hardCapacity} with one.
                </p>
              ) : null}

              {breakfastSettings && (
                <label style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12, marginTop: 4, cursor: 'pointer' }}>
                  <input
                    type="checkbox"
                    checked={breakfast}
                    onChange={(e) => setBreakfast(e.target.checked)}
                    style={{ width: 18, height: 18, margin: 0 }}
                  />
                  <span>Breakfast <span style={{ color: '#555' }}>(+${breakfastSettings.price}/person/night)</span></span>
                </label>
              )}

              {nights > 0 && (
                <div className="rd-price-summary">
                  <div className="rd-price-row">
                    <span>Room ({nights} night{nights !== 1 ? 's' : ''})</span>
                    <span>{quoteLoading ? '…' : `$${roomTotal.toFixed(2)}`}</span>
                  </div>
                  {/* CHANGED BY AI (2026-07-15): please review. Small transparency touch — shows
                      why the price is higher, if a seasonal rule and/or demand surge applied to any
                      night in this stay. */}
                  {quoteReasons.length > 0 && (
                    <p className="muted small" style={{ margin: '-2px 0 4px' }}>
                      Includes: {quoteReasons.join(', ')}
                    </p>
                  )}
                  {breakfast && breakfastSettings && (
                    <div className="rd-price-row"><span>Breakfast</span><span>${breakfastTotal}</span></div>
                  )}
                  {extraBedsNeeded > 0 && (
                    <div className="rd-price-row"><span>Extra bed{extraBedsNeeded === 1 ? '' : 's'}</span><span>${extraBedTotal.toFixed(2)}</span></div>
                  )}
                  <div className="rd-price-row rd-price-total"><span>Total</span><span>${grandTotal.toFixed(2)}</span></div>
                </div>
              )}

              {bookError && <p className="form-error" style={{ maxWidth: 'none' }}>{bookError}</p>}

              {currentUser ? (
                <button type="button" className="confirm-btn rd-book-btn" onClick={handleBookNow}>
                  Book Now
                </button>
              ) : (
                <Link to="/login" className="confirm-btn rd-book-btn" style={{ display: 'block', textAlign: 'center', textDecoration: 'none' }}>
                  Log in to Book
                </Link>
              )}
            </div>
          </aside>
        </div>
      )}
    </div>
  );
}
