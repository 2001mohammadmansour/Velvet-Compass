import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
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
  const { t } = useTranslation();
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
        <h2>{isEditing ? t('partners.form.editTitle') : t('partners.form.addTitle')}</h2>
        <form onSubmit={handleSubmit}>
          <label>
            {t('partners.form.name')}
            <input name="name" type="text" value={form.name} onChange={handleChange} required />
          </label>

          <label>
            {t('partners.form.city')}
            <select name="city" value={form.city} onChange={handleChange}>
              {cityOptions.map((city) => (
                <option key={city} value={city}>{city}</option>
              ))}
            </select>
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

  useEffect(() => {
    let mounted = true;
    getPartners()
      .then((data) => { if (mounted) setPartners(data); })
      .catch((err) => { if (mounted) setError(err.message || t('partners.loadError')); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [t]);

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
      <header className="facilities-hero">
        <div className="facilities-hero-copy">
          <p className="eyebrow">{t('partners.eyebrow')}</p>
          <h1>{t('partners.title')}</h1>
          <p className="hero-description">
            {t('partners.heroDescription')}
          </p>
        </div>

        <div className="facilities-hero-card">
          <span>{t('partners.partnersLabel')}</span>
          <strong>{t('partners.partnersCount', { count: partners.length })}</strong>
          <p>{t('partners.browseByCity')}</p>
          {isAdmin && (
            <button
              type="button"
              className={`admin-manage-toggle${manageMode ? " active" : ""}`}
              onClick={() => setManageMode((v) => !v)}
            >
              {manageMode ? t('partners.doneEditing') : t('partners.managePartners')}
            </button>
          )}
        </div>
      </header>

      <main className="facilities-layout">
        <section className="trips-section">
          <div className="section-head">
            <h2>{t('partners.allPartners')}</h2>
            <p>{t('partners.partnersAvailable', { count: visiblePartners.length })}</p>
          </div>

          {manageMode && (
            <button type="button" className="add-trip-btn" onClick={() => setShowAddForm(true)}>
              {t('partners.addPartner')}
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
              <button className="sr-clear-btn" onClick={clearFilters}>{t('common.clearAll')}</button>
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
