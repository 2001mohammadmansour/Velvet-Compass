import { useEffect, useMemo, useState } from "react";
import { getCurrentRole } from "./services/auth";
import { getPartners, createPartner, updatePartner, deletePartner, uploadPartnerPhoto } from "./services/partners";
import "./OurPartners.css";
import "./room.css";

const cityOptions = [
  "Damascus",
  "Aleppo",
  "Homs",
  "Hama",
  "Latakia",
  "Tartous",
  "Idlib",
  "Palmyra",
  "Bloudan",
  "Deir ez-Zor",
  "Qamishli",
  "Daraa",
  "As-Suwayda",
  "Raqqa",
  "Douma",
  "Quneitra",
];

const emptyPartner = {
  name: "",
  city: cityOptions[0],
  description: "",
};

function PartnerFormModal({ initialPartner, onSave, onCancel, saving }) {
  const [form, setForm] = useState(initialPartner || emptyPartner);
  const [file, setFile] = useState(null);
  const isEditing = Boolean(initialPartner);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.name.trim() || !form.description.trim()) return;
    onSave({ ...form, name: form.name.trim(), description: form.description.trim() }, file);
  };

  return (
    <div className="partner-form-overlay" onClick={onCancel}>
      <div className="partner-form-modal" onClick={(e) => e.stopPropagation()}>
        <h2>{isEditing ? "Edit partner" : "Add partner"}</h2>
        <form onSubmit={handleSubmit}>
          <label>
            Name
            <input name="name" type="text" value={form.name} onChange={handleChange} required />
          </label>

          <label>
            City
            <select name="city" value={form.city} onChange={handleChange}>
              {cityOptions.map((city) => (
                <option key={city} value={city}>{city}</option>
              ))}
            </select>
          </label>

          <label>
            Description
            <textarea name="description" rows={3} value={form.description} onChange={handleChange} required />
          </label>

          <label>
            Photo
            <input
              name="photo"
              type="file"
              accept="image/*"
              onChange={(e) => setFile(e.target.files?.[0] || null)}
            />
          </label>

          <div className="partner-form-actions">
            <button type="submit" className="partner-form-save" disabled={saving}>
              {saving ? "Saving..." : isEditing ? "Save changes" : "Add partner"}
            </button>
            <button type="button" className="partner-form-cancel" onClick={onCancel} disabled={saving}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default function OurPartners() {
  const isAdmin = useMemo(() => getCurrentRole() === "admin", []);

  const [partners, setPartners] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const [manageMode, setManageMode] = useState(false);
  const [editingPartner, setEditingPartner] = useState(null);
  const [showAddForm, setShowAddForm] = useState(false);

  useEffect(() => {
    let mounted = true;
    getPartners()
      .then((data) => { if (mounted) setPartners(data); })
      .catch((err) => { if (mounted) setError(err.message || 'Unable to load partners.'); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, []);

  const [selectedCity, setSelectedCity] = useState('');

  const visiblePartners = useMemo(() => {
    return partners.filter((partner) => !selectedCity || partner.city === selectedCity);
  }, [partners, selectedCity]);

  const clearFilters = () => setSelectedCity('');

  const handleAddPartner = async (newPartner, file) => {
    setSaving(true);
    try {
      let created = await createPartner(newPartner);
      if (file) created = await uploadPartnerPhoto(created.id, file);
      setPartners((prev) => [...prev, created]);
      setShowAddForm(false);
    } catch (err) {
      alert('Unable to add partner: ' + (err.message || err));
    } finally {
      setSaving(false);
    }
  };

  const handleEditPartner = async (updatedPartner, file) => {
    setSaving(true);
    try {
      let saved = await updatePartner(editingPartner.id, updatedPartner);
      if (file) saved = await uploadPartnerPhoto(editingPartner.id, file);
      setPartners((prev) => prev.map((p) => (p.id === editingPartner.id ? saved : p)));
      setEditingPartner(null);
    } catch (err) {
      alert('Unable to save partner: ' + (err.message || err));
    } finally {
      setSaving(false);
    }
  };

  const handleDeletePartner = async (partnerId) => {
    if (!window.confirm("Delete this partner? This cannot be undone.")) return;
    try {
      await deletePartner(partnerId);
      setPartners((prev) => prev.filter((p) => p.id !== partnerId));
    } catch (err) {
      alert('Unable to delete partner: ' + (err.message || err));
    }
  };

  if (loading) {
    return (
      <div className="facilities-page">
        <p className="muted" style={{ textAlign: 'center', padding: '80px 20px' }}>Loading partners...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="facilities-page">
        <p className="muted" style={{ textAlign: 'center', padding: '80px 20px', color: '#9b1c1c' }}>{error}</p>
      </div>
    );
  }

  return (
    <div className="facilities-page">
      <header className="facilities-hero">
        <div className="facilities-hero-copy">
          <p className="eyebrow">Our Partners</p>
          <h1>Our Partners</h1>
          <p className="hero-description">
            Restaurants, shops, and experiences we work with across Syria.
          </p>
        </div>

        <div className="facilities-hero-card">
          <span>Partners</span>
          <strong>{partners.length} partners</strong>
          <p>Browse our partners by city.</p>
          {isAdmin && (
            <button
              type="button"
              className={`admin-manage-toggle${manageMode ? " active" : ""}`}
              onClick={() => setManageMode((v) => !v)}
            >
              {manageMode ? "Done editing" : "✎ Manage partners"}
            </button>
          )}
        </div>
      </header>

      <main className="facilities-layout">
        <section className="trips-section">
          <div className="section-head">
            <h2>All Partners</h2>
            <p>{visiblePartners.length} partners available</p>
          </div>

          {manageMode && (
            <button type="button" className="add-trip-btn" onClick={() => setShowAddForm(true)}>
              + Add partner
            </button>
          )}

          <div className="trips-grid">
            {visiblePartners.map((partner, i) => (
              <article className="trip-card partner-card" key={partner.id} style={{ animationDelay: `${i * 0.06}s` }}>
                <div className="partner-card-photo">
                  {partner.imageUrl
                    ? <img src={partner.imageUrl} alt={partner.name} />
                    : <span className="partner-photo-placeholder">🏙️</span>
                  }
                </div>
                <h3>{partner.name}</h3>
                <p className="trip-city">{partner.city}</p>
                <p className="trip-description">{partner.description}</p>
                {manageMode && (
                  <div className="trip-card-admin-actions">
                    <button type="button" onClick={() => setEditingPartner(partner)}>
                      Edit
                    </button>
                    <button type="button" className="danger" onClick={() => handleDeletePartner(partner.id)}>
                      Delete
                    </button>
                  </div>
                )}
              </article>
            ))}
          </div>

          {visiblePartners.length === 0 && (
            <div className="empty-state">No partners match the selected city.</div>
          )}
        </section>

        <aside className="filters-panel">
          <div className="sr-sidebar-header">
            <span className="sr-sidebar-title">Filters</span>
            {selectedCity && (
              <button className="sr-clear-btn" onClick={clearFilters}>Clear all</button>
            )}
          </div>

          <div className="sr-filter-section">
            <span className="sr-filter-label">City</span>
            <select
              className="sr-filter-input"
              value={selectedCity}
              onChange={(e) => setSelectedCity(e.target.value)}
            >
              <option value="">All cities</option>
              {cityOptions.map((c) => (
                <option key={c} value={c}>{c}</option>
              ))}
            </select>
          </div>
        </aside>
      </main>

      {showAddForm && (
        <PartnerFormModal onSave={handleAddPartner} onCancel={() => setShowAddForm(false)} saving={saving} />
      )}
      {editingPartner && (
        <PartnerFormModal
          initialPartner={editingPartner}
          onSave={handleEditPartner}
          onCancel={() => setEditingPartner(null)}
          saving={saving}
        />
      )}
    </div>
  );
}
