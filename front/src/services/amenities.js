// CHANGED BY AI (2026-07-15): new service file for the amenities catalog feature. Reads by scope
// are public; catalog CRUD requires an Admin-role token.
import { apiRequest as request } from './apiClient';

export async function getAmenities(scope) {
  const result = await request(`/api/v1/amenities?scope=${encodeURIComponent(scope)}`);
  return Array.isArray(result) ? result : [];
}

// CHANGED BY AI (2026-07-15): please review. Admin-only: includes inactive amenities, so the
// catalog management screen has something to reactivate (getAmenities above is active-only, for
// the owner/guest picklists).
export async function getAllAmenities(scope) {
  const result = await request(`/api/v1/amenities/all?scope=${encodeURIComponent(scope)}`);
  return Array.isArray(result) ? result : [];
}

// CHANGED BY AI (2026-07-15): please review. Owners can also call this now (not just Admin) — if
// the admin-curated catalog is missing something a hotel needs, the owner can add it themselves.
// It joins the same shared catalog, so it becomes available to every owner from then on.
export async function createAmenity({ name, icon, scope }) {
  return request('/api/v1/amenities', {
    method: 'POST',
    body: JSON.stringify({ name, icon: icon || null, scope }),
  });
}

export async function updateAmenity(id, { name, icon, isActive }) {
  return request(`/api/v1/amenities/${id}`, {
    method: 'PUT',
    body: JSON.stringify({ name, icon: icon || null, isActive }),
  });
}

export async function deactivateAmenity(id) {
  return request(`/api/v1/amenities/${id}/deactivate`, { method: 'PATCH' });
}

export async function reactivateAmenity(id) {
  return request(`/api/v1/amenities/${id}/reactivate`, { method: 'PATCH' });
}
