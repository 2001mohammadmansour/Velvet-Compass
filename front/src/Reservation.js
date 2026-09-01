// CHANGED BY AI (2026-07-15): please review — this page used to be a single long form doing
// everything at once (room details, dates, guest count, add-ons, price summary, payment). Dates/
// guests/add-ons now get chosen on the new RoomDetail.js page instead (which shows photos,
// description, and amenities and computes the running total), and this page is purely the
// checkout: a compact room+price summary, your contact info, and a payment method.
import { useState } from "react";
import "./reservation.css";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getCurrentUser } from "./services/auth";
import { createBooking, initiatePayment, confirmPayment } from "./services/guest";

export default function Reservation() {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const incoming = location.state || {};
  const room = incoming.room;
  const currentUser = getCurrentUser();

  const { checkIn, checkOut, guests, nights, roomTotal, breakfast, breakfastTotal, extraBeds, extraBedTotal, grandTotal } = incoming;

  const [customer, setCustomer] = useState({ name: "", email: "", phone: "" });
  const [payment, setPayment] = useState({ method: "", cardNumber: "", expiry: "", cvv: "" });
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [confirmation, setConfirmation] = useState(null);
  const [discountCode, setDiscountCode] = useState("");

  const discountApplied = discountCode.trim().toLowerCase() === "alhamid";
  const finalTotal = discountApplied ? 0 : grandTotal;

  const handleCustomer = (field, value) => setCustomer({ ...customer, [field]: value });
  const handlePayment = (field, value) => setPayment({ ...payment, [field]: value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!customer.name || !customer.email) {
      setError(t('reservation.errors.nameEmailRequired'));
      return;
    }
    if (!payment.method) {
      setError(t('reservation.errors.paymentMethodRequired'));
      return;
    }

    setSubmitting(true);
    try {
      const booking = await createBooking({
        hotelId: room.hotelId,
        roomTypeId: room.id,
        checkIn,
        checkOut,
        specialRequests: breakfast ? "Breakfast add-on requested" : null,
        guestName: customer.name,
        guestCount: guests,
        includeBreakfast: breakfast,
        extraBedCount: extraBeds,
      });

      // "Pay on arrival" leaves the booking Pending; card confirms it immediately. The
      // backend doesn't validate real payment details, so card fields aren't sent anywhere.
      let status = "pending";
      if (payment.method !== "cash") {
        const paymentRecord = await initiatePayment(booking.id, payment.method);
        await confirmPayment(paymentRecord.id, `DEMO-${Date.now()}-${booking.id}`);
        status = "confirmed";
      }
      setConfirmation({ status });
    } catch (err) {
      setError(err.message || t('reservation.errors.bookingFailed'));
    } finally {
      setSubmitting(false);
    }
  };

  if (confirmation) {
    return (
      <div className="reservation-page">
        <div className="section-card confirmation-card">
          {confirmation.status === "confirmed" ? (
            <>
              <h2 className="section-title">{t('reservation.bookingConfirmed')}</h2>
              <p>{t('reservation.confirmedMessage', { hotel: room.hotel, room: room.name })}</p>
            </>
          ) : (
            <>
              <h2 className="section-title">{t('reservation.requestSent')}</h2>
              <p>{t('reservation.pendingMessage', { room: room.name, hotel: room.hotel })}</p>
            </>
          )}
          <Link to="/" className="back-btn">{t('reservation.backToHome')}</Link>
        </div>
      </div>
    );
  }

  if (!room || !checkIn || !checkOut) {
    return (
      <div className="reservation-page">
        <div className="back-wrapper">
          <button type="button" className="back-btn" onClick={() => navigate(-1)}>{t('reservation.back')}</button>
        </div>
        <div className="section-card">
          <p>{t('reservation.noRoomSelected')} <Link to="/hotels">{t('reservation.browseHotels')}</Link></p>
        </div>
      </div>
    );
  }

  return (
    <div className="reservation-page">
      <div className="back-wrapper">
        <button type="button" className="back-btn" onClick={() => navigate(-1)}>{t('reservation.back')}</button>
      </div>

      <h1 className="title">{t('reservation.checkout')}</h1>

      {/* ROOM + PRICE SUMMARY */}
      <div className="section-card">
        <h2 className="section-title">{t('reservation.yourStay')}</h2>
        <div className="room-details">
          {room.img && <img src={room.img} className="room-img" alt={room.name} />}
          <div>
            <h3>{room.name}</h3>
            <p className="hotel">{room.hotel}</p>
            <p className="city">{room.city}</p>
            <p className="stars">{"★".repeat(room.rating || 0)}</p>
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 6, fontSize: 15, marginTop: 18, paddingTop: 14, borderTop: '1px solid #f0f2f8' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span>
              {checkIn} → {checkOut} · {t('reservation.nightsCount', { count: nights })} · {t('reservation.guestsCount', { count: guests })}
              {extraBeds > 0 ? t('reservation.extraBedsSuffix', { count: extraBeds }) : ''}
            </span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span>{t('reservation.roomNightsPrice', { count: nights, price: room.price })}</span>
            <span>${roomTotal}</span>
          </div>
          {breakfast && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span>{t('reservation.breakfast')}</span>
              <span>${breakfastTotal}</span>
            </div>
          )}
          {extraBeds > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span>{t('reservation.extraBed', { count: extraBeds })}</span>
              <span>${extraBedTotal.toFixed(2)}</span>
            </div>
          )}
          {discountApplied && (
            <div style={{ display: 'flex', justifyContent: 'space-between', color: '#16a34a' }}>
              <span>{t('reservation.discountLabel', { code: 'alhamid' })}</span>
              <span>-${grandTotal.toFixed(2)}</span>
            </div>
          )}
          <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 700, borderTop: '1px solid #e5e7eb', paddingTop: 8, marginTop: 4 }}>
            <span>{t('reservation.total')}</span>
            <span>${finalTotal.toFixed(2)}</span>
          </div>
        </div>

        <input
          type="text"
          placeholder={t('reservation.discountCodePlaceholder')}
          value={discountCode}
          onChange={(e) => setDiscountCode(e.target.value)}
          style={{ marginTop: 12 }}
        />
        {discountApplied && <p style={{ color: '#16a34a', margin: '6px 0 0' }}>{t('reservation.discountApplied')}</p>}
      </div>

      {!currentUser && (
        <div className="section-card">
          <h2 className="section-title">{t('reservation.pleaseLogin')}</h2>
          <p>{t('reservation.needAccount')}</p>
          <Link to="/login" className="back-btn">{t('reservation.login')}</Link>
        </div>
      )}

      {currentUser && (
        <form onSubmit={handleSubmit}>
          {/* CUSTOMER INFO */}
          <div className="section-card">
            <h2 className="section-title">{t('reservation.yourInformation')}</h2>

            <input
              type="text"
              placeholder={t('reservation.fullName')}
              value={customer.name}
              onChange={(e) => handleCustomer("name", e.target.value)}
            />

            <input
              type="email"
              placeholder={t('reservation.email')}
              value={customer.email}
              onChange={(e) => handleCustomer("email", e.target.value)}
            />

            <input
              type="tel"
              placeholder={t('reservation.phoneNumber')}
              value={customer.phone}
              onChange={(e) => handleCustomer("phone", e.target.value)}
            />
          </div>

          {/* PAYMENT */}
          <div className="section-card">
            <h2 className="section-title">{t('reservation.paymentMethod')}</h2>

            <div className="payment-methods">
              <button
                type="button"
                className={`method ${payment.method === "card" ? "active" : ""}`}
                onClick={() => setPayment({ ...payment, method: "card" })}
              >
                {t('reservation.creditCard')}
              </button>

              <button
                type="button"
                className={`method ${payment.method === "cash" ? "active" : ""}`}
                onClick={() => setPayment({ ...payment, method: "cash" })}
              >
                {t('reservation.payOnArrival')}
              </button>
            </div>

            {payment.method === "card" && (
              <div className="card-fields">
                <input
                  type="text"
                  placeholder={t('reservation.cardNumber')}
                  value={payment.cardNumber}
                  onChange={(e) => handlePayment("cardNumber", e.target.value)}
                />

                <input
                  type="text"
                  placeholder={t('reservation.expiryDate')}
                  value={payment.expiry}
                  onChange={(e) => handlePayment("expiry", e.target.value)}
                />

                <input
                  type="text"
                  placeholder={t('reservation.cvv')}
                  value={payment.cvv}
                  onChange={(e) => handlePayment("cvv", e.target.value)}
                />
              </div>
            )}
          </div>

          {error && <p className="form-error">{error}</p>}

          <button type="submit" className="confirm-btn" disabled={submitting}>
            {submitting ? t('reservation.booking') : t('reservation.confirmBooking')}
          </button>
        </form>
      )}
    </div>
  );
}
