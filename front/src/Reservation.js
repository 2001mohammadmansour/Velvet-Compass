// CHANGED BY AI (2026-07-15): please review — this page used to be a single long form doing
// everything at once (room details, dates, guest count, add-ons, price summary, payment). Dates/
// guests/add-ons now get chosen on the new RoomDetail.js page instead (which shows photos,
// description, and amenities and computes the running total), and this page is purely the
// checkout: a compact room+price summary, your contact info, and a payment method.
import { useState } from "react";
import "./reservation.css";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { getCurrentUser } from "./services/auth";
import { createBooking, initiatePayment, confirmPayment } from "./services/guest";

export default function Reservation() {
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

  const handleCustomer = (field, value) => setCustomer({ ...customer, [field]: value });
  const handlePayment = (field, value) => setPayment({ ...payment, [field]: value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!customer.name || !customer.email) {
      setError("Please enter your name and email.");
      return;
    }
    if (!payment.method) {
      setError("Please select a payment method.");
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

      // "Pay on arrival" leaves the booking Pending; card/PayPal confirm it immediately. The
      // backend doesn't validate real payment details, so card fields aren't sent anywhere.
      let status = "pending";
      if (payment.method !== "cash") {
        const paymentRecord = await initiatePayment(booking.id, payment.method);
        await confirmPayment(paymentRecord.id, `DEMO-${Date.now()}-${booking.id}`);
        status = "confirmed";
      }
      setConfirmation({ status });
    } catch (err) {
      setError(err.message || "Unable to complete booking. Please try again.");
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
              <h2 className="section-title">Booking confirmed!</h2>
              <p>Your reservation at {room.hotel} is confirmed. We've saved your details for {room.name}.</p>
            </>
          ) : (
            <>
              <h2 className="section-title">Booking request sent</h2>
              <p>Your request for {room.name} at {room.hotel} is pending the hotel's approval. You'll be notified once it's reviewed.</p>
            </>
          )}
          <Link to="/" className="back-btn">Back to home</Link>
        </div>
      </div>
    );
  }

  if (!room || !checkIn || !checkOut) {
    return (
      <div className="reservation-page">
        <div className="back-wrapper">
          <button type="button" className="back-btn" onClick={() => navigate(-1)}>← Back</button>
        </div>
        <div className="section-card">
          <p>No room selected. <Link to="/hotels">Browse hotels</Link></p>
        </div>
      </div>
    );
  }

  return (
    <div className="reservation-page">
      <div className="back-wrapper">
        <button type="button" className="back-btn" onClick={() => navigate(-1)}>← Back</button>
      </div>

      <h1 className="title">Checkout</h1>

      {/* ROOM + PRICE SUMMARY */}
      <div className="section-card">
        <h2 className="section-title">Your Stay</h2>
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
            <span>{checkIn} → {checkOut} · {nights} night{nights !== 1 ? 's' : ''} · {guests} guest{guests !== 1 ? 's' : ''}{extraBeds > 0 ? ` · ${extraBeds} extra bed${extraBeds === 1 ? '' : 's'}` : ''}</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span>Room ({nights} night{nights !== 1 ? 's' : ''} × ${room.price})</span>
            <span>${roomTotal}</span>
          </div>
          {breakfast && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span>Breakfast</span>
              <span>${breakfastTotal}</span>
            </div>
          )}
          {extraBeds > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span>Extra bed{extraBeds === 1 ? '' : 's'}</span>
              <span>${extraBedTotal.toFixed(2)}</span>
            </div>
          )}
          <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 700, borderTop: '1px solid #e5e7eb', paddingTop: 8, marginTop: 4 }}>
            <span>Total</span>
            <span>${grandTotal.toFixed(2)}</span>
          </div>
        </div>
      </div>

      {!currentUser && (
        <div className="section-card">
          <h2 className="section-title">Please log in to book</h2>
          <p>You need an account to complete a booking.</p>
          <Link to="/login" className="back-btn">Log in</Link>
        </div>
      )}

      {currentUser && (
        <form onSubmit={handleSubmit}>
          {/* CUSTOMER INFO */}
          <div className="section-card">
            <h2 className="section-title">Your Information</h2>

            <input
              type="text"
              placeholder="Full Name"
              value={customer.name}
              onChange={(e) => handleCustomer("name", e.target.value)}
            />

            <input
              type="email"
              placeholder="Email"
              value={customer.email}
              onChange={(e) => handleCustomer("email", e.target.value)}
            />

            <input
              type="tel"
              placeholder="Phone Number"
              value={customer.phone}
              onChange={(e) => handleCustomer("phone", e.target.value)}
            />
          </div>

          {/* PAYMENT */}
          <div className="section-card">
            <h2 className="section-title">Payment Method</h2>

            <div className="payment-methods">
              <button
                type="button"
                className={`method ${payment.method === "card" ? "active" : ""}`}
                onClick={() => setPayment({ ...payment, method: "card" })}
              >
                Credit Card
              </button>

              <button
                type="button"
                className={`method ${payment.method === "paypal" ? "active" : ""}`}
                onClick={() => setPayment({ ...payment, method: "paypal" })}
              >
                PayPal
              </button>

              <button
                type="button"
                className={`method ${payment.method === "cash" ? "active" : ""}`}
                onClick={() => setPayment({ ...payment, method: "cash" })}
              >
                Pay on Arrival
              </button>
            </div>

            {payment.method === "card" && (
              <div className="card-fields">
                <input
                  type="text"
                  placeholder="Card Number"
                  value={payment.cardNumber}
                  onChange={(e) => handlePayment("cardNumber", e.target.value)}
                />

                <input
                  type="text"
                  placeholder="Expiry Date (MM/YY)"
                  value={payment.expiry}
                  onChange={(e) => handlePayment("expiry", e.target.value)}
                />

                <input
                  type="text"
                  placeholder="CVV"
                  value={payment.cvv}
                  onChange={(e) => handlePayment("cvv", e.target.value)}
                />
              </div>
            )}
          </div>

          {error && <p className="form-error">{error}</p>}

          <button type="submit" className="confirm-btn" disabled={submitting}>
            {submitting ? "Booking..." : "Confirm Booking"}
          </button>
        </form>
      )}
    </div>
  );
}
