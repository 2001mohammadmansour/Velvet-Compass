// CHANGED: this file used to have its own local fetch wrapper (duplicated across every service
// file); now uses the shared client in apiClient.js instead, which also handles automatic
// access-token refresh on a 401.
import { apiRequest as request } from './apiClient';

export async function getHotels() {
  const result = await request("/api/v1/hotels");
  const items = Array.isArray(result?.items) ? result.items : [];
  return items.map((h) => ({
    hotelId: h.hotelId,
    hotelName: h.name,
    city: h.city,
    country: h.country,
    stars: h.starRating,
    cardPhoto: h.primaryImageUrl || null,
  }));
}

// Returns the hotels owned by the currently logged-in user (requires an Owner-role token).
export async function getMyHotels() {
  const result = await request('/api/v1/hotels/my');
  const items = Array.isArray(result) ? result : [];
  return items.map((h) => ({
    hotelId: h.hotelId,
    hotelName: h.name,
    city: h.city,
    country: h.country,
    stars: h.starRating,
    cardPhoto: h.primaryImageUrl || null,
  }));
}

// CHANGED BY AI (2026-07-15): please review. New: full hotel detail (used for the hotel-level
// amenities header on Rooms.js — HotelSummaryDto/getHotels above deliberately omits amenities to
// avoid bloating the multi-hotel list payload, so the single-hotel page fetches detail directly).
export async function getHotelById(hotelId) {
  const h = await request(`/api/v1/hotels/${hotelId}`);
  return {
    hotelId: h.hotelId,
    hotelName: h.name,
    description: h.description || '',
    city: h.city,
    country: h.country,
    stars: h.starRating,
    amenities: Array.isArray(h.amenities) ? h.amenities : [],
  };
}

// CHANGED BY AI (2026-07-13): please review — rewired from a mock "/api/stats" endpoint that
// never existed on the real backend (the homepage's stats section silently failed and always
// showed "—") to the real platform-wide counts.
export async function getStats() {
  const s = await request("/api/v1/stats");
  return {
    hotels: s?.hotels || 0,
    cities: s?.cities || 0,
    bookings: s?.bookings || 0,
    rooms: s?.rooms || 0,
  };
}

// Platform-wide revenue/booking stats and top-hotel rankings (Admin-only). Real backend data —
// simpler than the old mock version (no per-room drill-down, no monthly chart per hotel, no
// star-rating editing, since none of those have backend support).
// CHANGED BY AI (2026-07-15): please review. year/month let Hotels Analytics filter to a specific
// month or year (bookings scoped by check-in date server-side); omit both for all-time.
export async function getAdminDashboard({ year, month } = {}) {
  const params = new URLSearchParams();
  if (year) params.set('year', year);
  if (month) params.set('month', month);
  const qs = params.toString();
  return request(`/api/v1/admin/dashboard${qs ? `?${qs}` : ''}`);
}

// Admin-only list of every user, with booking activity and hotels owned (for owners).
export async function getAdminUsers() {
  const result = await request('/api/v1/users');
  const list = Array.isArray(result) ? result : [];
  return list.map((u) => ({
    id: u.id,
    username: u.username,
    email: u.email,
    phoneNumber: u.phoneNumber,
    role: u.role,
    createdAt: u.createdAt,
    bookingsCount: u.bookingsCount,
    amountPaidToPlatform: u.amountPaidToPlatform,
    ownedHotelNames: Array.isArray(u.ownedHotelNames) ? u.ownedHotelNames : [],
    isSuspended: Boolean(u.isSuspended),
    suspendedUntil: u.suspendedUntil,
  }));
}

// A specific user's bookings, for the admin user-detail drill-down.
export async function getAdminUserBookings(userId) {
  const result = await request(`/api/v1/users/${userId}/bookings`);
  const list = Array.isArray(result) ? result : [];
  return list.map((b) => ({
    id: b.id,
    hotelName: b.hotelName,
    checkIn: b.checkinDate,
    checkOut: b.checkoutDate,
    status: String(b.status || '').toLowerCase(),
    totalAmount: b.totalAmount,
    // CHANGED BY AI (2026-07-13): real review data for the Reviews feature (previously the admin
    // panel always showed a "not available yet" placeholder here).
    hasReview: Boolean(b.hasReview),
    reviewScore: b.reviewScore ?? null,
    reviewComment: b.reviewComment || '',
    reviewId: b.reviewId ?? null,
  }));
}

// CHANGED BY AI (2026-07-13): new admin review-moderation functions — full review detail, and
// deleting a review (e.g. if it's toxic/abusive). Used by both the admin Users tab and the Hotels
// Analytics room drill-down.
export async function getReviewDetail(reviewId) {
  return request(`/api/v1/reviews/${reviewId}`);
}

export async function deleteReview(reviewId) {
  return request(`/api/v1/reviews/${reviewId}`, { method: 'DELETE' });
}

// `until`: an ISO date string, or omit/null for an indefinite suspension.
export async function suspendUser(userId, until) {
  return request(`/api/v1/users/${userId}/suspend`, {
    method: 'POST',
    body: JSON.stringify({ until: until || null }),
  });
}

export async function unsuspendUser(userId) {
  return request(`/api/v1/users/${userId}/unsuspend`, { method: 'POST' });
}

const MONTH_LABELS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

// Platform-wide equivalent of the owner dashboard's getRevenueStats. The backend has no single
// "all bookings across every hotel" endpoint, so this fetches every hotel, then each hotel's
// bookings, and aggregates them here — same approach as the owner version, just summed across
// hotels instead of scoped to one. Revenue is attributed to each booking's check-in date.
// CHANGED BY AI (2026-07-15): please review. This used to sum totalAmount — the full amount
// guests paid — which is gross booking volume, not platform revenue (it was ~6x too high vs. the
// "Platform Revenue" figure on Overview/Hotels Analytics, which correctly uses the 15% booking
// fee). Now sums each booking's actual platformFee instead, so this tab is finally consistent
// with the rest of the admin dashboard's definition of "platform revenue".
export async function getAdminRevenueStats(filters = {}) {
  const hotelsResult = await request('/api/v1/hotels?pageSize=1000');
  const hotelIds = (hotelsResult?.items || []).map((h) => h.hotelId);
  const perHotelBookings = await Promise.all(
    hotelIds.map((id) => request(`/api/v1/bookings/hotel/${id}`).catch(() => []))
  );
  const raw = perHotelBookings.flat();

  const paid = raw
    .filter((b) => b.status === 'Confirmed' || b.status === 'Completed')
    .map((b) => {
      const [y, m, d] = String(b.checkinDate).split('-').map(Number);
      return { stayDate: new Date(y, m - 1, d), amount: Number(b.platformFee) || 0 };
    });

  const sumRange = (from, to) =>
    paid.filter((b) => b.stayDate >= from && b.stayDate < to).reduce((s, b) => s + b.amount, 0);

  const year = Number(filters.year) || new Date().getFullYear();
  const month = Number(filters.month) || 0;
  const quarter = Number(filters.quarter) || Math.floor(month / 3) + 1;
  const quarterStartMonth = (quarter - 1) * 3;
  const mode = filters.mode || 'monthly';
  const now = new Date();

  const summary = {
    monthly: sumRange(new Date(year, month, 1), new Date(year, month + 1, 1)),
    quarterly: sumRange(new Date(year, quarterStartMonth, 1), new Date(year, quarterStartMonth + 3, 1)),
    yearly: sumRange(new Date(year, 0, 1), new Date(year + 1, 0, 1)),
    ytd: sumRange(new Date(year, 0, 1), new Date(year, now.getMonth(), now.getDate() + 1)),
    custom: 0,
  };
  let points = [];

  if (mode === 'monthly') {
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    for (let d = 1; d <= daysInMonth; d += 1) {
      points.push({ label: String(d), value: sumRange(new Date(year, month, d), new Date(year, month, d + 1)) });
    }
  } else if (mode === 'quarterly') {
    for (let i = 0; i < 3; i += 1) {
      const m = quarterStartMonth + i;
      points.push({ label: MONTH_LABELS[m], value: sumRange(new Date(year, m, 1), new Date(year, m + 1, 1)) });
    }
  } else if (mode === 'yearly') {
    for (let m = 0; m < 12; m += 1) {
      points.push({ label: MONTH_LABELS[m], value: sumRange(new Date(year, m, 1), new Date(year, m + 1, 1)) });
    }
  } else if (mode === 'ytd') {
    for (let m = 0; m <= now.getMonth(); m += 1) {
      points.push({ label: MONTH_LABELS[m], value: sumRange(new Date(year, m, 1), new Date(year, m + 1, 1)) });
    }
  } else {
    // custom
    const start = filters.startDate ? new Date(filters.startDate) : new Date(0);
    const end = filters.endDate ? new Date(filters.endDate) : new Date();
    const cursor = new Date(start);
    while (cursor <= end) {
      const from = new Date(cursor);
      const to = new Date(cursor);
      to.setDate(to.getDate() + 1);
      points.push({ label: `${from.getMonth() + 1}/${from.getDate()}`, value: sumRange(from, to) });
      cursor.setDate(cursor.getDate() + 1);
    }
    summary.custom = points.reduce((s, p) => s + p.value, 0);
  }

  return { summary, points };
}

// CHANGED BY AI (2026-07-13): please review — rewired from a mock "/api/rooms/search" endpoint
// that never existed on the real backend (the homepage search returned nothing/errored) to the
// real cross-hotel search.
// CHANGED BY AI (2026-07-15): please review. checkIn/checkOut are now sent to the backend, which
// excludes room types with zero rooms available for those dates and reports how many remain
// (availableCount) — backs the "sold out rooms hidden" / "only N left" UI in SearchResults.js.
export async function searchRooms({ checkIn, checkOut } = {}) {
  const params = new URLSearchParams();
  if (checkIn) params.set('checkIn', checkIn);
  if (checkOut) params.set('checkOut', checkOut);
  const qs = params.toString();
  const items = await request(`/api/v1/room-types/search${qs ? `?${qs}` : ''}`);
  const list = Array.isArray(items) ? items : [];
  return list.map((rt) => ({
    id: rt.id,
    hotelId: rt.hotelId,
    name: rt.name,
    hotelName: rt.hotelName,
    city: rt.city,
    country: rt.country,
    hotelStars: rt.hotelStars,
    price: rt.price,
    capacity: rt.capacity,
    photos: rt.primaryImageUrl ? [rt.primaryImageUrl] : [],
    avgScore: rt.avgScore ?? null,
    reviewCount: rt.reviewCount || 0,
    // CHANGED BY AI (2026-07-15): please review. Description + extra-bed scalar fields (cheap,
    // no join). Amenities deliberately excluded here — this is a cross-hotel, potentially-many-rows
    // list; see getRoomTypesForHotel below for where amenities ARE included (single hotel, few rows).
    description: rt.description || '',
    allowExtraBed: Boolean(rt.allowExtraBed),
    maxExtraBeds: Number(rt.maxExtraBeds) || 0,
    extraBedPriceType: rt.extraBedPriceType === 'Fixed' ? 'fixed' : 'percentage',
    extraBedPriceForOneBed: Number(rt.extraBedPriceForOneBed) || 0,
    extraBedPriceForTwoBeds: Number(rt.extraBedPriceForTwoBeds) || 0,
    availableCount: Number(rt.availableCount) || 0,
  }));
}

// CHANGED BY AI (2026-07-15): please review. New: full room type detail (public endpoint) — used
// by the new RoomDetail.js page for its photo gallery (the full Images list, not just one
// primaryImageUrl) plus fresh description/amenities/extra-bed settings.
export async function getRoomTypeDetail(hotelId, roomTypeId) {
  const rt = await request(`/api/v1/hotel/${hotelId}/room-types/${roomTypeId}`);
  return {
    id: rt.id,
    hotelId: rt.hotelId,
    name: rt.name,
    description: rt.description || '',
    capacity: rt.capacity,
    beds: rt.beds,
    price: rt.basePrice,
    images: Array.isArray(rt.images) ? [...rt.images].sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0)) : [],
    amenities: Array.isArray(rt.amenities) ? rt.amenities : [],
    allowExtraBed: Boolean(rt.allowExtraBed),
    maxExtraBeds: Number(rt.maxExtraBeds) || 0,
    extraBedPriceType: rt.extraBedPriceType === 'Fixed' ? 'fixed' : 'percentage',
    extraBedPriceForOneBed: Number(rt.extraBedPriceForOneBed) || 0,
    extraBedPriceForTwoBeds: Number(rt.extraBedPriceForTwoBeds) || 0,
  };
}

// Returns the room types (used as "rooms" throughout the guest UI) for one specific hotel.
// CHANGED BY AI (2026-07-15): please review. checkIn/checkOut are now sent to the backend, same
// as searchRooms above — sold-out room types are excluded and availableCount is reported.
export async function getRoomTypesForHotel(hotelId, { checkIn, checkOut } = {}) {
  const params = new URLSearchParams();
  if (checkIn) params.set('checkIn', checkIn);
  if (checkOut) params.set('checkOut', checkOut);
  const qs = params.toString();
  const items = await request(`/api/v1/hotel/${hotelId}/room-types${qs ? `?${qs}` : ''}`);
  const list = Array.isArray(items) ? items : [];
  return list.map((rt) => ({
    id: rt.id,
    hotelId,
    name: rt.name,
    price: rt.basePrice,
    capacity: rt.capacity,
    photos: rt.primaryImageUrl ? [rt.primaryImageUrl] : [],
    // CHANGED BY AI (2026-07-13): real review stats for the Reviews feature (previously always
    // hardcoded null/0 since the backend had no reviews at all).
    avgScore: rt.avgScore ?? null,
    reviewCount: rt.reviewCount || 0,
    // CHANGED BY AI (2026-07-15): please review. Description, room-type amenities, and the
    // extra-bed system settings — a hotel typically has only a handful of room types, so amenities
    // (a collection) are safe to include here (unlike the cross-hotel searchRooms above).
    description: rt.description || '',
    amenities: Array.isArray(rt.amenities) ? rt.amenities : [],
    allowExtraBed: Boolean(rt.allowExtraBed),
    maxExtraBeds: Number(rt.maxExtraBeds) || 0,
    extraBedPriceType: rt.extraBedPriceType === 'Fixed' ? 'fixed' : 'percentage',
    extraBedPriceForOneBed: Number(rt.extraBedPriceForOneBed) || 0,
    extraBedPriceForTwoBeds: Number(rt.extraBedPriceForTwoBeds) || 0,
    availableCount: Number(rt.availableCount) || 0,
  }));
}

export async function approveHotel(email, stars) {
  return request('/api/admin/approve-hotel', {
    method: 'PATCH',
    body: JSON.stringify({ email, stars: stars || undefined }),
  });
}

// Creates the real Hotel record for a pending owner request, using what they submitted at
// sign-up (hotelName/city/stars/phoneNumber). Description/Address aren't collected at sign-up,
// so they start blank; Country defaults to "Syria" since sign-up only offers Syrian cities.
export async function createHotelForOwner(pendingRequest) {
  const changes = pendingRequest?.changes || {};
  const payload = {
    ownerEmail: pendingRequest?.ownerEmail,
    name: changes.hotelName || '',
    description: '',
    address: '',
    city: changes.city || '',
    country: 'Syria',
    starRating: Number(changes.stars) || 3,
    phone: changes.phoneNumber || '',
    email: pendingRequest?.ownerEmail || '',
  };
  return request('/api/v1/hotels', { method: 'POST', body: JSON.stringify(payload) });
}

const hotelsService = { getHotels, getMyHotels, getHotelById, getRoomTypeDetail, getAdminDashboard, getAdminRevenueStats, getAdminUsers, getAdminUserBookings, suspendUser, unsuspendUser, searchRooms, getRoomTypesForHotel, approveHotel, createHotelForOwner, getReviewDetail, deleteReview };
export default hotelsService;
