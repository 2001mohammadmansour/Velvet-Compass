import { apiRequest as request } from './apiClient';

// Admin: platform-wide commission in three buckets, plus the per-hotel breakdown.
//  - pendingTotal:    unsettled bookings — stay hasn't ended, 15% not due yet
//  - toCollectTotal:  stay ended / cancelled — the hotel is holding the 15% and must pay it
//  - collectedTotal:  paid + admin-confirmed — money the platform actually has
export async function getCommissionOverview() {
  const d = await request('/api/v1/commission/overview');
  return {
    pendingTotal: Number(d?.pendingTotal) || 0,
    toCollectTotal: Number(d?.toCollectTotal) || 0,
    collectedTotal: Number(d?.collectedTotal) || 0,
    hotels: Array.isArray(d?.hotels) ? d.hotels : [],
  };
}

// Admin: confirm a hotel's owner-claimed commission payment actually arrived.
export async function confirmCommission(hotelId) {
  return request(`/api/v1/commission/hotel/${hotelId}/confirm`, { method: 'POST' });
}

// Admin: the claimed payment never arrived — goes back to "owed".
export async function rejectCommission(hotelId) {
  return request(`/api/v1/commission/hotel/${hotelId}/reject`, { method: 'POST' });
}

// Admin: write the claimed commission off — no money moves.
export async function waiveCommission(hotelId) {
  return request(`/api/v1/commission/hotel/${hotelId}/waive`, { method: 'POST' });
}
