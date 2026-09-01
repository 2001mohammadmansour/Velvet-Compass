import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { getCurrentRole } from "./services/auth";
import {
  getPartners, createPartner, updatePartner, deletePartner, uploadPartnerPhoto,
  registerPartnerClick, PARTNER_CATEGORIES,
} from "./services/partners";
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
  cities: [],
  description: "",
  category: PARTNER_CATEGORIES[0],
  websiteUrl: "",
};

const formatCities = (cities) => (Array.isArray(cities) ? cities.join(" · ") : "");

function PartnerFormModal({ initialPartner, onSave, onCancel, saving }) {
  const { t } = useTranslation();
  const [form, setForm] = useState({ ...emptyPartner, ...(initialPartner || {}) });
  const [file, setFile] = useState(null);
  const isEditing = Boolean(initialPartner);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const toggleCity = (city) => {
    setForm((prev) => ({
      ...prev,
      cities: prev.cities.includes(city)
        ? prev.cities.filter((c) => c !== city)
        : [...prev.cities, city],
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.name.trim() || !form.description.trim() || form.cities.length === 0) return;
    onSave(
      { ...form, name: form.name.trim(), description: form.description.trim(), websiteUrl: form.websiteUrl.trim() },
      file,
    );
  };

  return (
    <div className="partner-form-overlay" onClick={onCancel}>
      <div className="partner-form-modal" onClick={(e) => e.stopPropagation()}>
        <h2>{isEditing ? t('partners.form.editTitle') : t('partners.form.addTitle')}</h2>
        <form onSubmit={handleSubmit}>
          <label>
            {t('partners.form.name')}
            <input name="name" type="text" value={form.name} onChange={handleChange} required />
          </label>

          <label>
            {t('partners.form.category')}
            <select name="category" value={form.category} onChange={handleChange}>
              {PARTNER_CATEGORIES.map((c) => (
                <option key={c} value={c}>{t(`partners.categories.${c}`)}</option>
              ))}
            </select>
          </label>

          <div className="partner-cities-field">
            <span className="partner-cities-label">{t('partners.form.cities')}</span>
            <div className="partner-cities-grid">
              {cityOptions.map((city) => (
                <label key={city} className="partner-city-check">
                  <input
                    type="checkbox"
                    checked={form.cities.includes(city)}
                    onChange={() => toggleCity(city)}
                  />
                  <span>{city}</span>
                </label>
              ))}
            </div>
          </div>

          <label>
            {t('partners.form.website')}
            <input
              name="websiteUrl"
              type="text"
              placeholder="https://example.com"
              value={form.websiteUrl}
              onChange={handleChange}
            />
          </label>

          <label>
            {t('partners.form.description')}
            <textarea name="description" rows={3} value={form.description} onChange={handleChange} required />
          </label>

          <label>
            {t('partners.form.photo')}
            <input
              name="photo"
              type="file"
              accept="image/*"
              onChange={(e) => setFile(e.target.files?.[0] || null)}
            />
          </label>

          <div className="partner-form-actions">
            <button type="submit" className="partner-form-save" disabled={saving}>
              {saving ? t('partners.form.saving') : isEditing ? t('common.save') : t('partners.form.addSubmit')}
            </button>
            <button type="button" className="partner-form-cancel" onClick={onCancel} disabled={saving}>
              {t('common.cancel')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default function OurPartners() {
  const { t } = useTranslation();
  const isAdmin = useMemo(() => getCurrentRole() === "admin", []);

  const [partners, setPartners] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const [manageMode, setManageMode] = useState(false);
  const [editingPartner, setEditingPartner] = useState(null);
  const [showAddForm, setShowAddForm] = useState(false);

  const [activeCategory, setActiveCategory] = useState(PARTNER_CATEGORIES[0]);
  const [selectedCity, setSelectedCity] = useState('');

  useEffect(() => {
    let mounted = true;
    getPartners()
      .then((data) => { if (mounted) setPartners(data); })
      .catch((err) => { if (mounted) setError(err.message || t('partners.loadError')); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [t]);

  const countByCategory = useMemo(() => {
    const counts = {};
    for (const p of partners) counts[p.category] = (counts[p.category] || 0) + 1;
    return counts;
  }, [partners]);

  const visiblePartners = useMemo(() => {
    return partners.filter((p) =>
      p.category === activeCategory &&
      (!selectedCity || (p.cities || []).includes(selectedCity)));
  }, [partners, activeCategory, selectedCity]);

  const openWebsite = (partner) => {
    if (!partner.websiteUrl) return;
    registerPartnerClick(partner.id);
    window.open(partner.websiteUrl, "_blank", "noopener,noreferrer");
  };

  const handleAddPartner = async (newPartner, file) => {
    setSaving(true);
    try {
      let created = await createPartner(newPartner);
      if (file) created = await uploadPartnerPhoto(created.id, file);
      setPartners((prev) => [...prev, created]);
      setActiveCategory(created.category);
      setShowAddForm(false);
    } catch (err) {
      alert(t('partners.addError') + (err.message || err));
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
      alert(t('partners.saveError') + (err.message || err));
    } finally {
      setSaving(false);
    }
  };

  const handleDeletePartner = async (partnerId) => {
    if (!window.confirm(t('partners.confirmDelete'))) return;
    try {
      await deletePartner(partnerId);
      setPartners((prev) => prev.filter((p) => p.id !== partnerId));
    } catch (err) {
      alert(t('partners.deleteError') + (err.message || err));
    }
  };

  if (loading) {
    return (
      <div className="facilities-page">
        <p className="muted" style={{ textAlign: 'center', padding: '80px 20px' }}>{t('partners.loading')}</p>
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
      <div className="partners-topbar">
        <h1>{t('partners.title')}</h1>
        {isAdmin && (
          <button
            type="button"
            className={`admin-manage-toggle inline${manageMode ? " active" : ""}`}
            onClick={() => setManageMode((v) => !v)}
          >
            {manageMode ? t('partners.doneEditing') : t('partners.managePartners')}
          </button>
        )}
      </div>

      <nav className="partner-tabs" role="tablist">
        {PARTNER_CATEGORIES.map((cat) => (
          <button
            key={cat}
            type="button"
            role="tab"
            aria-selected={activeCategory === cat}
            className={`partner-tab${activeCategory === cat ? " active" : ""}`}
            onClick={() => setActiveCategory(cat)}
          >
            {t(`partners.categories.${cat}`)}
            {countByCategory[cat] ? <span className="partner-tab-count">{countByCategory[cat]}</span> : null}
          </button>
        ))}
      </nav>

      <main className="facilities-layout">
        <section className="trips-section">
          {manageMode && (
            <button type="button" className="add-trip-btn" onClick={() => setShowAddForm(true)}>
              {t('partners.addPartner')}
            </button>
          )}

          <div className="trips-grid">
            {visiblePartners.map((partner, i) => (
              <article
                className={`trip-card partner-card${!manageMode && partner.websiteUrl ? ' partner-card-clickable' : ''}`}
                key={partner.id}
                style={{ animationDelay: `${i * 0.06}s` }}
                onClick={!manageMode && partner.websiteUrl ? () => openWebsite(partner) : undefined}
              >
                <div className="partner-card-photo">
                  {partner.imageUrl
                    ? <img src={partner.imageUrl} alt={partner.name} />
                    : <span className="partner-photo-placeholder">🏙️</span>
                  }
                </div>
                <h3>{partner.name}</h3>
                <p className="trip-city">📍 {formatCities(partner.cities)}</p>
                <p className="trip-description">{partner.description}</p>
                {partner.websiteUrl && !manageMode && (
                  <span className="partner-card-link-hint">{t('partners.detail.visitWebsite')} ↗</span>
                )}
                {manageMode && (
                  <div className="trip-card-admin-actions">
                    <button type="button" onClick={() => setEditingPartner(partner)}>
                      {t('common.edit')}
                    </button>
                    <button type="button" className="danger" onClick={() => handleDeletePartner(partner.id)}>
                      {t('common.delete')}
                    </button>
                  </div>
                )}
              </article>
            ))}
          </div>

          {visiblePartners.length === 0 && (
            <div className="empty-state">{t('partners.noMatch')}</div>
          )}
        </section>

        <aside className="filters-panel">
          <div className="sr-sidebar-header">
            <span className="sr-sidebar-title">{t('common.filters')}</span>
            {selectedCity && (
              <button className="sr-clear-btn" onClick={() => setSelectedCity('')}>{t('common.clearAll')}</button>
            )}
          </div>

          <div className="sr-filter-section">
            <span className="sr-filter-label">{t('common.city')}</span>
            <select
              className="sr-filter-input"
              value={selectedCity}
              onChange={(e) => setSelectedCity(e.target.value)}
            >
              <option value="">{t('common.allCities')}</option>
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
