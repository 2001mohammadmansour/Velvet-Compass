import { apiRequest as request } from './apiClient';

// Admin: per-hotel preview of everything that would settle right now.
export async function getSettlementPreview() {
  const r = await request('/api/v1/settlements/preview');
  return Array.isArray(r) ? r : [];
}

// Admin: run the settlement for one hotel.
export async function runSettlement(hotelId, periodLabel) {
  return request('/api/v1/settlements/run', {
    method: 'POST',
    body: JSON.stringify({ hotelId, periodLabel: periodLabel || null }),
  });
}

// Admin: full settlement history (optionally one hotel).
export async function getSettlementHistory(hotelId) {
  const qs = hotelId ? `?hotelId=${hotelId}` : '';
  const r = await request(`/api/v1/settlements${qs}`);
  return Array.isArray(r) ? r : [];
}

// Owner/Admin: one hotel's payout history.
export async function getHotelSettlements(hotelId) {
  const r = await request(`/api/v1/settlements/hotel/${hotelId}`);
  return Array.isArray(r) ? r : [];
}
