// CHANGED BY AI (2026-07-15): please review. Hotel-scoped pricing service — seasonal price rules
// and occupancy price tiers are managed once per hotel (not per room type), plus the public
// guest-facing price quote (still per room type, since a guest picks a room type, not a hotel).
import { apiRequest as request } from './apiClient';

const base = (hotelId) => `/api/v1/hotel/${hotelId}/pricing`;

export async function getPriceQuote(hotelId, roomTypeId, checkIn, checkOut) {
  return request(`/api/v1/hotel/${hotelId}/room-types/${roomTypeId}/pricing/quote?checkIn=${checkIn}&checkOut=${checkOut}`);
}

export async function getSeasonalRules(hotelId) {
  const result = await request(`${base(hotelId)}/seasonal-rules`);
  return Array.isArray(result) ? result : [];
}

export async function createSeasonalRule(hotelId, { name, startDate, endDate, adjustmentType, adjustmentValue }) {
  return request(`${base(hotelId)}/seasonal-rules`, {
    method: 'POST',
    body: JSON.stringify({ name, startDate, endDate, adjustmentType, adjustmentValue: Number(adjustmentValue) || 0 }),
  });
}

export async function updateSeasonalRule(hotelId, ruleId, { name, startDate, endDate, adjustmentType, adjustmentValue }) {
  return request(`${base(hotelId)}/seasonal-rules/${ruleId}`, {
    method: 'PUT',
    body: JSON.stringify({ name, startDate, endDate, adjustmentType, adjustmentValue: Number(adjustmentValue) || 0 }),
  });
}

export async function deleteSeasonalRule(hotelId, ruleId) {
  return request(`${base(hotelId)}/seasonal-rules/${ruleId}`, { method: 'DELETE' });
}

export async function getOccupancyTiers(hotelId) {
  const result = await request(`${base(hotelId)}/occupancy-tiers`);
  return Array.isArray(result) ? result : [];
}

export async function createOccupancyTier(hotelId, { minOccupancyPercent, adjustmentType, adjustmentValue }) {
  return request(`${base(hotelId)}/occupancy-tiers`, {
    method: 'POST',
    body: JSON.stringify({ minOccupancyPercent: Number(minOccupancyPercent) || 0, adjustmentType, adjustmentValue: Number(adjustmentValue) || 0 }),
  });
}

export async function updateOccupancyTier(hotelId, tierId, { minOccupancyPercent, adjustmentType, adjustmentValue }) {
  return request(`${base(hotelId)}/occupancy-tiers/${tierId}`, {
    method: 'PUT',
    body: JSON.stringify({ minOccupancyPercent: Number(minOccupancyPercent) || 0, adjustmentType, adjustmentValue: Number(adjustmentValue) || 0 }),
  });
}

export async function deleteOccupancyTier(hotelId, tierId) {
  return request(`${base(hotelId)}/occupancy-tiers/${tierId}`, { method: 'DELETE' });
}
