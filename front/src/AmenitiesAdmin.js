import { useEffect, useState } from 'react';
import { getAllAmenities, createAmenity, updateAmenity, deactivateAmenity, reactivateAmenity } from './services/amenities';

// CHANGED BY AI (2026-07-15): new admin screen for the amenities catalog (hotel-level and
// room-type-level benefits, e.g. free wifi/parking/gym vs. sea view/minibar/balcony). Structured
// like AdminUsers.js: a load() function, inline add/edit forms, and a table per scope. Amenities
// are never hard-deleted — "Deactivate" just hides them from the owner/guest picklists without
// breaking hotels/room types that already reference them.
const EMPTY_FORM = { name: '' };

function ScopeSection({ scope, title, hint }) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [addForm, setAddForm] = useState(EMPTY_FORM);
  const [adding, setAdding] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState(EMPTY_FORM);
  const [savingId, setSavingId] = useState(null);

  function load() {
    setLoading(true);
    setError('');
    return getAllAmenities(scope)
      .then(setItems)
      .catch((err) => setError(err.message || 'Unable to load amenities.'))
      .finally(() => setLoading(false));
  }

  useEffect(() => { load(); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  async function handleAdd(e) {
    e.preventDefault();
    if (!addForm.name.trim()) return;
    setAdding(true);
    try {
      await createAmenity({ name: addForm.name.trim(), icon: null, scope });
      setAddForm(EMPTY_FORM);
      await load();
    } catch (err) {
      alert('Unable to add amenity: ' + (err.message || err));
    } finally {
      setAdding(false);
    }
  }

  function startEdit(item) {
    setEditingId(item.id);
    setEditForm({ name: item.name });
  }

  async function saveEdit(item) {
    setSavingId(item.id);
    try {
      await updateAmenity(item.id, { name: editForm.name.trim(), icon: null, isActive: item.isActive });
      setEditingId(null);
      await load();
    } catch (err) {
      alert('Unable to save amenity: ' + (err.message || err));
    } finally {
      setSavingId(null);
    }
  }

  async function handleDeactivate(item) {
    if (!window.confirm(`Deactivate "${item.name}"? It will disappear from owner/guest picklists but stays linked to any hotel or room type that already uses it.`)) return;
    setSavingId(item.id);
    try {
      await deactivateAmenity(item.id);
      await load();
    } catch (err) {
      alert('Unable to deactivate amenity: ' + (err.message || err));
    } finally {
      setSavingId(null);
    }
  }

  async function handleReactivate(item) {
    setSavingId(item.id);
    try {
      await reactivateAmenity(item.id);
      await load();
    } catch (err) {
      alert('Unable to reactivate amenity: ' + (err.message || err));
    } finally {
      setSavingId(null);
    }
  }

  return (
    <div className="admin-card" style={{ marginBottom: 20 }}>
      <div className="admin-card-title">{title}</div>
      <p className="admin-stat-sub" style={{ marginTop: -4, marginBottom: 12 }}>{hint}</p>

      <form onSubmit={handleAdd} style={{ display: 'flex', gap: 8, marginBottom: 16, flexWrap: 'wrap' }}>
        <input
          className="sr-filter-input"
          style={{ maxWidth: 220 }}
          placeholder="Amenity name (e.g. Free WiFi)"
          value={addForm.name}
          onChange={(e) => setAddForm((f) => ({ ...f, name: e.target.value }))}
        />
        <button type="submit" className="ha-sort-btn active" disabled={adding || !addForm.name.trim()}>
          {adding ? 'Adding...' : '+ Add'}
        </button>
      </form>

      {loading && <p className="muted small">Loading...</p>}
      {error && <p className="muted small" style={{ color: '#e05555' }}>{error}</p>}

      {!loading && !error && (
        <div className="ha-table-wrap">
          <table className="ha-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  {editingId === item.id ? (
                    <>
                      <td>
                        <input
                          className="sr-filter-input"
                          value={editForm.name}
                          onChange={(e) => setEditForm((f) => ({ ...f, name: e.target.value }))}
                        />
                      </td>
                      <td>
                        <span className={`booking-status booking-status-${item.isActive ? 'confirmed' : 'cancelled'}`}>
                          {item.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        <div style={{ display: 'flex', gap: 6 }}>
                          <button className="ha-sort-btn active" disabled={savingId === item.id} onClick={() => saveEdit(item)}>
                            {savingId === item.id ? '...' : 'Save'}
                          </button>
                          <button className="ha-sort-btn" onClick={() => setEditingId(null)}>Cancel</button>
                        </div>
                      </td>
                    </>
                  ) : (
                    <>
                      <td><strong>{item.name}</strong></td>
                      <td>
                        <span className={`booking-status booking-status-${item.isActive ? 'confirmed' : 'cancelled'}`}>
                          {item.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        <div style={{ display: 'flex', gap: 6 }}>
                          <button className="ha-sort-btn" onClick={() => startEdit(item)}>Edit</button>
                          {item.isActive ? (
                            <button className="ha-sort-btn" disabled={savingId === item.id} onClick={() => handleDeactivate(item)}>
                              {savingId === item.id ? '...' : 'Deactivate'}
                            </button>
                          ) : (
                            <button className="ha-sort-btn active" disabled={savingId === item.id} onClick={() => handleReactivate(item)}>
                              {savingId === item.id ? '...' : 'Reactivate'}
                            </button>
                          )}
                        </div>
                      </td>
                    </>
                  )}
                </tr>
              ))}
              {items.length === 0 && (
                <tr><td colSpan={3} className="ha-hint">No amenities yet — add one above.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default function AmenitiesAdmin() {
  return (
    <div className="ha-root">
      <ScopeSection
        scope="Hotel"
        title="🏨 Hotel Amenities"
        hint="Shown on a hotel's own page — wifi, parking, gym, pool, etc."
      />
      <ScopeSection
        scope="RoomType"
        title="🛏️ Room Amenities"
        hint="Shown on a specific room type — sea view, minibar, balcony, etc."
      />
    </div>
  );
}
