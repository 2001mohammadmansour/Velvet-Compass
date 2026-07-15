import { useEffect, useMemo, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import './room.css'
import { getRoomTypesForHotel, getHotelById } from './services/hotels'
import { getRoomReviews } from './services/guest'
import { getCurrentRole } from './services/auth'
import PriceRangeSlider from './PriceRangeSlider'

const CAT_LABELS = {
  staff: 'Staff', location: 'Location', facilities: 'Facilities',
  cleanliness: 'Cleanliness', comfort: 'Comfort', value: 'Value',
}

const SCORE_OPTIONS = [
  { label: 'Any', value: 0 },
  { label: '6+', value: 6 },
  { label: '7+', value: 7 },
  { label: '8+', value: 8 },
  { label: '9+', value: 9 },
]

const SORT_OPTIONS = [
  { value: 'recommended', label: 'Recommended' },
  { value: 'price_asc',   label: 'Price: Low → High' },
  { value: 'price_desc',  label: 'Price: High → Low' },
  { value: 'score_desc',  label: 'Review Score' },
]

function ReviewsModal({ room, onClose }) {
  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getRoomReviews(room.hotelId, room.id)
      .then(setData)
      .catch(() => setData(null))
      .finally(() => setLoading(false))
  }, [room.hotelId, room.id])

  return (
    <div className="rv-overlay" onClick={onClose}>
      <div className="rvv-modal" onClick={e => e.stopPropagation()}>
        <div className="rv-modal-header">
          <div>
            <h3>{room.name}</h3>
            <p>{room.hotelName} · {room.city}</p>
            {data && data.reviewCount > 0 && (
              <p style={{ marginTop: 4 }}>
                <span className="rvv-avg">{data.avgScore}</span>
                <span className="rvv-count">/ 10 · {data.reviewCount} review{data.reviewCount !== 1 ? 's' : ''}</span>
              </p>
            )}
          </div>
          <button className="rv-close" onClick={onClose} aria-label="Close">×</button>
        </div>

        {loading && <p style={{ color: '#6b7280' }}>Loading reviews…</p>}

        {!loading && data?.categoryAverages && (
          <div className="rvv-cats">
            {Object.entries(CAT_LABELS).map(([key, label]) => (
              <div key={key} className="rvv-cat-pill">
                <span>{label}</span>
                <span>{data.categoryAverages[key]}</span>
              </div>
            ))}
          </div>
        )}

        {!loading && data && (
          <div className="rvv-list">
            {data.reviews.length === 0 && <p style={{ color: '#9ca3af' }}>No reviews yet.</p>}
            {data.reviews.map(r => (
              <div key={r.id} className="rvv-item">
                <div className="rvv-item-header">
                  <span className="rvv-item-name">{r.guestName}</span>
                  <span className="rvv-item-score">{r.overallScore}/10</span>
                  <span className="rvv-item-date">{new Date(r.createdAt).toLocaleDateString()}</span>
                </div>
                <p className="rvv-item-comment">{r.comment}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

export default function Rooms() {
  const location = useLocation()
  const navigate = useNavigate()
  const isOwner = getCurrentRole() === 'hotel_owner'

  const incoming = location.state || {}
  const selectedHotel = incoming.selectedHotel || ''
  const hotel = incoming.hotel || null
  const checkIn  = incoming.checkIn  || ''
  const checkOut = incoming.checkOut || ''

  const handleBack = () => {
    const fallback = isOwner ? '/ownerhome' : '/hotels'
    if (window.history.length > 1) navigate(-1)
    else navigate(fallback, { replace: true })
  }

  const [allRooms, setAllRooms] = useState([])
  const [loading, setLoading]   = useState(true)
  const [error, setError]       = useState('')
  const [reviewsRoom, setReviewsRoom] = useState(null)
  // CHANGED BY AI (2026-07-15): please review. New: hotel-level amenities, fetched via a direct
  // detail call since HotelSummaryDto/the `hotel` nav-state object don't carry them.
  const [hotelAmenities, setHotelAmenities] = useState([])

  // Filters
  const [roomName, setRoomName]     = useState('')
  const [guests, setGuests]         = useState(incoming.guests || 1)
  const [priceMin, setPriceMin]     = useState(null)   // null = no filter
  const [priceMax, setPriceMax]     = useState(null)   // null = no filter
  const [minScore, setMinScore]     = useState(0)
  const [sortBy, setSortBy]         = useState('recommended')

  // Guard: redirect to /hotels if no hotel was selected (e.g. direct URL navigation)
  useEffect(() => {
    if (!selectedHotel) navigate('/hotels', { replace: true })
  }, [selectedHotel, navigate])

  useEffect(() => {
    let mounted = true
    const hid = hotel?.hotelId
    if (!hid) { setAllRooms([]); setLoading(false); return }
    setLoading(true)
    setError('')
    getRoomTypesForHotel(hid)
      .then(data  => { if (mounted) setAllRooms(Array.isArray(data) ? data : []) })
      .catch(err  => { if (mounted) setError(err.message || 'Unable to load rooms.') })
      .finally(() => { if (mounted) setLoading(false) })
    getHotelById(hid)
      .then(data => { if (mounted) setHotelAmenities(Array.isArray(data?.amenities) ? data.amenities : []) })
      .catch(() => { if (mounted) setHotelAmenities([]) })
    return () => { mounted = false }
  }, [hotel?.hotelId])

  // Rooms that belong to this hotel
  const hotelKey = String(
    hotel?.hotelId || hotel?.hotelName || selectedHotel || ''
  ).trim().toLowerCase()

  const hotelRooms = useMemo(() =>
    allRooms.filter(r =>
      String(r.hotelId || '').toLowerCase() === hotelKey ||
      String(r.hotelName || '').toLowerCase() === hotelKey
    ),
    [allRooms, hotelKey]
  )

  const maxPrice = useMemo(() => {
    if (!hotelRooms.length) return 1000
    return Math.ceil(Math.max(...hotelRooms.map(r => r.price)) / 100) * 100
  }, [hotelRooms])

  const activeFilterCount = useMemo(() => {
    let n = 0
    if (roomName)            n++
    if (priceMin !== null)   n++
    if (priceMax !== null)   n++
    if (minScore)            n++
    if (guests > 1)          n++
    return n
  }, [roomName, priceMin, priceMax, minScore, guests])

  const clearFilters = () => {
    setRoomName('')
    setPriceMin(null)
    setPriceMax(null)
    setMinScore(0)
    setGuests(1)
    setSortBy('recommended')
  }

  const results = useMemo(() => {
    let filtered = hotelRooms.filter(r => {
      const okName    = !roomName || String(r.name || '').toLowerCase().includes(roomName.toLowerCase())
      // CHANGED BY AI (2026-07-15): please review — this used to only check base capacity, so a
      // 2-person room that can sleep 4 with 2 extra beds was wrongly excluded from a 3+ guest
      // search. Now checks effective capacity (base + max extra beds, if the room allows them).
      const effectiveCapacity = r.capacity + (r.allowExtraBed ? (r.maxExtraBeds || 0) : 0)
      const okGuests  = !guests || effectiveCapacity >= Number(guests)
      const okPrMin   = priceMin === null || r.price >= priceMin
      const okPrMax   = priceMax === null || r.price <= priceMax
      const okScore   = minScore === 0 || (r.avgScore !== null && r.avgScore >= minScore)
      return okName && okGuests && okPrMin && okPrMax && okScore
    })

    if (sortBy === 'price_asc')  filtered.sort((a, b) => a.price - b.price)
    if (sortBy === 'price_desc') filtered.sort((a, b) => b.price - a.price)
    if (sortBy === 'score_desc') filtered.sort((a, b) => (b.avgScore || 0) - (a.avgScore || 0))

    return filtered
  }, [hotelRooms, roomName, guests, priceMin, priceMax, minScore, sortBy])

  // CHANGED BY AI (2026-07-15): please review — this used to jump straight to checkout
  // (/reservation). Now it opens the new room detail/product page (photos, description,
  // amenities, live-priced booking sidebar); Reservation.js is reached only from there, once
  // dates/guests/add-ons are already decided.
  const handleViewRoom = (room) => {
    navigate('/room-detail', {
      state: {
        room: { id: room.id, hotelId: room.hotelId },
        checkIn, checkOut, guests,
      },
    })
  }

  const hotelStars = hotel?.stars || (hotelRooms[0]?.hotelStars ?? null)
  const hotelCity  = hotel?.city  || (hotelRooms[0]?.city ?? '')

  return (
    <div className="sr-page">
      {/* Top bar */}
      <div className="sr-topbar">
        <button type="button" className="back-btn" onClick={handleBack}>← Back</button>
        <div className="sr-topbar-center">
          <h1 className="sr-title">
            {selectedHotel ? `Rooms at ${selectedHotel}` : 'Available Rooms'}
          </h1>
          {hotelCity && (
            <span className="sr-dates">
              📍 {hotelCity}
              {hotelStars ? `  ·  ${'★'.repeat(hotelStars)}` : ''}
            </span>
          )}
          {/* CHANGED BY AI (2026-07-15): please review. New hotel-level amenities row. */}
          {hotelAmenities.length > 0 && (
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, marginTop: 6 }}>
              {hotelAmenities.map((a) => (
                <span key={a.id} className="sr-dates" style={{ fontSize: 12 }}>
                  {a.icon ? `${a.icon} ` : ''}{a.name}
                </span>
              ))}
            </div>
          )}
        </div>
        <input
          className="sr-filter-input"
          style={{ width: 200 }}
          placeholder="Search room name…"
          value={roomName}
          onChange={e => setRoomName(e.target.value)}
        />
        {activeFilterCount > 0 && (
          <button className="sr-clear-btn" onClick={clearFilters}>
            Clear all <span className="sr-filter-badge">{activeFilterCount}</span>
          </button>
        )}
      </div>

      {loading && <p className="sr-status">Loading rooms…</p>}
      {error   && <p className="sr-status sr-error">{error}</p>}

      <div className="sr-body">
        {/* Sidebar */}
        <aside className="sr-sidebar">
          <div className="sr-sidebar-header">
            <span className="sr-sidebar-title">Filters</span>
            {activeFilterCount > 0 && (
              <button className="sr-clear-btn" onClick={clearFilters}>Clear all</button>
            )}
          </div>

          {/* Guests */}
          <div className="sr-filter-section">
            <label className="sr-filter-label">Guests</label>
            <input
              className="sr-filter-input"
              type="number"
              min={1}
              value={guests}
              onChange={e => setGuests(Number(e.target.value) || 1)}
            />
          </div>

          {/* Sort */}
          <div className="sr-filter-section">
            <label className="sr-filter-label">Sort by</label>
            <div className="sr-sort-list">
              {SORT_OPTIONS.map(opt => (
                <button
                  key={opt.value}
                  className={`sr-sort-btn${sortBy === opt.value ? ' active' : ''}`}
                  onClick={() => setSortBy(opt.value)}
                >
                  {opt.label}
                </button>
              ))}
            </div>
          </div>

          {/* Price */}
          <div className="sr-filter-section">
            <label className="sr-filter-label">Price per night</label>
            <PriceRangeSlider
              min={0}
              max={maxPrice}
              valueMin={priceMin ?? 0}
              valueMax={priceMax ?? maxPrice}
              disabled={loading}
              onChange={({ min: lo, max: hi }) => {
                setPriceMin(lo > 0 ? lo : null)
                setPriceMax(hi < maxPrice ? hi : null)
              }}
            />
          </div>

          {/* Review Score */}
          <div className="sr-filter-section">
            <label className="sr-filter-label">Guest Review Score</label>
            <div className="sr-score-row">
              {SCORE_OPTIONS.map(opt => (
                <button
                  key={opt.value}
                  className={`sr-score-btn${minScore === opt.value ? ' active' : ''}`}
                  onClick={() => setMinScore(opt.value)}
                >
                  {opt.label}
                </button>
              ))}
            </div>
          </div>
        </aside>

        {/* Results */}
        <main className="sr-results">
          {!loading && (
            <p className="sr-count">
              {results.length} {results.length === 1 ? 'room' : 'rooms'} found
              {guests > 1 && <> · {guests} guests</>}
            </p>
          )}

          <div className="sr-grid">
            {results.map(room => (
              // CHANGED BY AI (2026-07-15): please review — the whole card now opens the new room
              // detail page (see handleViewRoom); the review badge stops propagation so it still
              // opens just the reviews modal instead of also navigating away.
              <div key={room.id} className="sr-card" onClick={() => handleViewRoom(room)} style={{ cursor: 'pointer' }}>
                {room.photos?.[0]
                  ? <img className="sr-card-img" src={room.photos[0]} alt={room.name} />
                  : <div className="sr-card-img-placeholder" />
                }
                <div className="sr-card-body">
                  <h3 className="sr-card-name">{room.name}</h3>

                  {room.reviewCount > 0 ? (
                    <button className="rv-badge" onClick={(e) => { e.stopPropagation(); setReviewsRoom(room); }}>
                      ★ {room.avgScore}/10 · {room.reviewCount} review{room.reviewCount !== 1 ? 's' : ''}
                    </button>
                  ) : (
                    <p className="sr-card-no-reviews">No reviews yet</p>
                  )}

                  {/* CHANGED BY AI (2026-07-15): please review. New room description (truncated)
                      and room-type amenity chips — the backend already had a Description field,
                      this is the first time it's actually shown to guests. */}
                  {room.description && (
                    <p className="muted small" style={{ margin: '4px 0' }}>
                      {room.description.length > 100 ? `${room.description.slice(0, 100)}…` : room.description}
                    </p>
                  )}
                  {Array.isArray(room.amenities) && room.amenities.length > 0 && (
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginBottom: 6 }}>
                      {room.amenities.map((a) => (
                        <span key={a.id} className="sr-dates" style={{ fontSize: 11 }}>
                          {a.icon ? `${a.icon} ` : ''}{a.name}
                        </span>
                      ))}
                    </div>
                  )}

                  <div className="sr-card-footer">
                    <p className="sr-card-price">
                      <span className="sr-price-amount">${room.price}</span>
                      <span className="sr-price-night"> / night</span>
                      {/* CHANGED BY AI (2026-07-15): please review — clarifies that a room's
                          listed capacity can be extended with an (automatic) extra bed, since the
                          guest filter above now matches on that effective capacity too. */}
                      <span className="sr-capacity">
                        {' '}· Sleeps {room.capacity}{room.allowExtraBed && room.maxExtraBeds > 0 ? ` (up to ${room.capacity + room.maxExtraBeds} with extra bed)` : ''}
                      </span>
                    </p>
                    <button className="sr-book-btn" onClick={(e) => { e.stopPropagation(); handleViewRoom(room); }}>View Details</button>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {!loading && !error && results.length === 0 && (
            <div className="sr-empty">
              {hotelRooms.length === 0 ? (
                <>
                  <p style={{ fontSize: 40, marginBottom: 8 }}>🏨</p>
                  <p style={{ fontWeight: 700, fontSize: 18, color: '#1a2340' }}>No rooms listed yet</p>
                  <p style={{ color: '#6b7280', marginTop: 4 }}>This hotel hasn't added any rooms.</p>
                  <button className="sr-clear-btn-lg" onClick={() => navigate('/hotels')} style={{ marginTop: 16 }}>
                    Browse other hotels
                  </button>
                </>
              ) : (
                <>
                  <p style={{ fontSize: 40, marginBottom: 8 }}>🔍</p>
                  <p style={{ fontWeight: 700, fontSize: 18, color: '#1a2340' }}>No rooms match your filters</p>
                  <p style={{ color: '#6b7280', marginTop: 4 }}>Try adjusting or clearing your filters.</p>
                  {activeFilterCount > 0 && (
                    <button className="sr-clear-btn-lg" onClick={clearFilters} style={{ marginTop: 16 }}>
                      Clear all filters
                    </button>
                  )}
                </>
              )}
            </div>
          )}
        </main>
      </div>

      {reviewsRoom && (
        <ReviewsModal room={reviewsRoom} onClose={() => setReviewsRoom(null)} />
      )}
    </div>
  )
}
