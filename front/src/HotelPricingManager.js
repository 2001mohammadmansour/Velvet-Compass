// CHANGED BY AI (2026-07-15): please review. Hotel-scoped pricing manager (moved from per-room-type
// — was RoomPricingManager.js) — lists and manages a hotel's seasonal price rules (holidays/high
// season) and occupancy price tiers (demand-based surge), each with inline add/edit/delete. Applies
// to every room type in the hotel; rendered once in OwnerDashboard.js's hotel settings, not per room.
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  getSeasonalRules, createSeasonalRule, updateSeasonalRule, deleteSeasonalRule,
  getOccupancyTiers, createOccupancyTier, updateOccupancyTier, deleteOccupancyTier,
} from './services/pricing';

const EMPTY_SEASONAL = { name: '', startDate: '', endDate: '', adjustmentType: 'Percentage', adjustmentValue: 0 };
const EMPTY_TIER = { minOccupancyPercent: '', adjustmentType: 'Percentage', adjustmentValue: 0 };

function formatAdjustment(type, value) {
  if (type === 'Percentage') return `${value > 0 ? '+' : ''}${value}%`;
  return `${value > 0 ? '+$' : '-$'}${Math.abs(value)}`;
}

function AdjustmentFields({ form, setForm }) {
  const { t } = useTranslation();
  return (
    <>
      <select value={form.adjustmentType} onChange={(e) => setForm((f) => ({ ...f, adjustmentType: e.target.value }))}>
        <option value="Percentage">{t('pricingManager.percentOfBase')}</option>
        <option value="Flat">{t('pricingManager.flatPerNight')}</option>
      </select>
      <input
        type="number"
        placeholder={form.adjustmentType === 'Percentage' ? 'e.g. 20' : 'e.g. 15.00'}
        value={form.adjustmentValue}
        onChange={(e) => setForm((f) => ({ ...f, adjustmentValue: e.target.value }))}
        style={{ width: 90 }}
      />
    </>
  );
}

function SeasonalRulesSection({ hotelId }) {
  const { t } = useTranslation();
  const [rules, setRules] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState(EMPTY_SEASONAL);
  const [saving, setSaving] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState(EMPTY_SEASONAL);

  function load() {
    setLoading(true);
    setError('');
    return getSeasonalRules(hotelId)
      .then(setRules)
      .catch((err) => setError(err.message || t('pricingManager.loadRulesError')))
      .finally(() => setLoading(false));
  }
  useEffect(() => { load(); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  async function handleAdd(e) {
    e.preventDefault();
    if (!form.name.trim() || !form.startDate || !form.endDate) return;
    setSaving(true);
    try {
      await createSeasonalRule(hotelId, form);
      setForm(EMPTY_SEASONAL);
      await load();
    } catch (err) {
      alert(t('pricingManager.addRuleError') + (err.message || err));
    } finally {
      setSaving(false);
    }
  }

  function startEdit(rule) {
    setEditingId(rule.id);
    setEditForm({ name: rule.name, startDate: rule.startDate, endDate: rule.endDate, adjustmentType: rule.adjustmentType, adjustmentValue: rule.adjustmentValue });
  }

  async function saveEdit(ruleId) {
    setSaving(true);
    try {
      await updateSeasonalRule(hotelId, ruleId, editForm);
      setEditingId(null);
      await load();
    } catch (err) {
      alert(t('pricingManager.saveRuleError') + (err.message || err));
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(ruleId) {
    if (!window.confirm(t('pricingManager.deleteRuleConfirm'))) return;
    try {
      await deleteSeasonalRule(hotelId, ruleId);
      await load();
    } catch (err) {
      alert(t('pricingManager.deleteRuleError') + (err.message || err));
    }
  }

  return (
    <div className="campaign-section">
      <label>{t('pricingManager.seasonalLabel')}</label>
      {loading && <p className="muted small">{t('pricingManager.loading')}</p>}
      {error && <p className="muted small" style={{ color: '#e05555' }}>{error}</p>}

      {!loading && rules.map((r) => (
        <div key={r.id} style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap', marginBottom: 8 }}>
          {editingId === r.id ? (
            <>
              <input value={editForm.name} onChange={(e) => setEditForm((f) => ({ ...f, name: e.target.value }))} placeholder={t('pricingManager.name')} style={{ width: 160 }} />
              <input type="date" value={editForm.startDate} onChange={(e) => setEditForm((f) => ({ ...f, startDate: e.target.value }))} />
              <input type="date" min={editForm.startDate} value={editForm.endDate} onChange={(e) => setEditForm((f) => ({ ...f, endDate: e.target.value }))} />
              <AdjustmentFields form={editForm} setForm={setEditForm} />
              <button type="button" className="campaign-next" disabled={saving} onClick={() => saveEdit(r.id)}>{t('pricingManager.save')}</button>
              <button type="button" className="campaign-back" onClick={() => setEditingId(null)}>{t('pricingManager.cancel')}</button>
            </>
          ) : (
            <>
              <span style={{ flex: 1, minWidth: 200 }}>
                <strong>{r.name}</strong> · {r.startDate} → {r.endDate} · {formatAdjustment(r.adjustmentType, r.adjustmentValue)}
              </span>
              <button type="button" className="campaign-back" onClick={() => startEdit(r)}>{t('pricingManager.edit')}</button>
              <button type="button" className="room-form-actions-delete room-form-actions-btn" onClick={() => handleDelete(r.id)}>{t('pricingManager.delete')}</button>
            </>
          )}
        </div>
      ))}
      {!loading && rules.length === 0 && <p className="muted small">{t('pricingManager.noRulesYet')}</p>}

      <form onSubmit={handleAdd} style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap', marginTop: 8 }}>
        <input placeholder={t('pricingManager.namePlaceholder')} value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} style={{ width: 160 }} />
        <input type="date" value={form.startDate} onChange={(e) => setForm((f) => ({ ...f, startDate: e.target.value }))} />
        <input type="date" min={form.startDate} value={form.endDate} onChange={(e) => setForm((f) => ({ ...f, endDate: e.target.value }))} />
        <AdjustmentFields form={form} setForm={setForm} />
        <button type="submit" className="campaign-next" disabled={saving}>{t('pricingManager.add')}</button>
      </form>
    </div>
  );
}

function OccupancyTiersSection({ hotelId }) {
  const { t } = useTranslation();
  const [tiers, setTiers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState(EMPTY_TIER);
  const [saving, setSaving] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState(EMPTY_TIER);

  function load() {
    setLoading(true);
    setError('');
    return getOccupancyTiers(hotelId)
      .then(setTiers)
      .catch((err) => setError(err.message || t('pricingManager.loadTiersError')))
      .finally(() => setLoading(false));
  }
  useEffect(() => { load(); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  async function handleAdd(e) {
    e.preventDefault();
    if (form.minOccupancyPercent === '' || Number(form.minOccupancyPercent) < 1) return;
    setSaving(true);
    try {
      await createOccupancyTier(hotelId, form);
      setForm(EMPTY_TIER);
      await load();
    } catch (err) {
      alert(t('pricingManager.addTierError') + (err.message || err));
    } finally {
      setSaving(false);
    }
  }

  function startEdit(tier) {
    setEditingId(tier.id);
    setEditForm({ minOccupancyPercent: tier.minOccupancyPercent, adjustmentType: tier.adjustmentType, adjustmentValue: tier.adjustmentValue });
  }

  async function saveEdit(tierId) {
    setSaving(true);
    try {
      await updateOccupancyTier(hotelId, tierId, editForm);
      setEditingId(null);
      await load();
    } catch (err) {
      alert(t('pricingManager.saveTierError') + (err.message || err));
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(tierId) {
    if (!window.confirm(t('pricingManager.deleteTierConfirm'))) return;
    try {
      await deleteOccupancyTier(hotelId, tierId);
      await load();
    } catch (err) {
      alert(t('pricingManager.deleteTierError') + (err.message || err));
    }
  }

  return (
    <div className="campaign-section">
      <label>{t('pricingManager.demandLabel')}</label>
      {loading && <p className="muted small">{t('pricingManager.loading')}</p>}
      {error && <p className="muted small" style={{ color: '#e05555' }}>{error}</p>}

      {!loading && tiers.map((tier) => (
        <div key={tier.id} style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap', marginBottom: 8 }}>
          {editingId === tier.id ? (
            <>
              <span>{t('pricingManager.at')}</span>
              <input
                type="number" min={1} max={100} style={{ width: 70 }}
                value={editForm.minOccupancyPercent}
                onChange={(e) => setEditForm((f) => ({ ...f, minOccupancyPercent: e.target.value }))}
              />
              <span>{t('pricingManager.occupied')}</span>
              <AdjustmentFields form={editForm} setForm={setEditForm} />
              <button type="button" className="campaign-next" disabled={saving} onClick={() => saveEdit(tier.id)}>{t('pricingManager.save')}</button>
              <button type="button" className="campaign-back" onClick={() => setEditingId(null)}>{t('pricingManager.cancel')}</button>
            </>
          ) : (
            <>
              <span style={{ flex: 1, minWidth: 200 }}>
                {t('pricingManager.at')} <strong>{tier.minOccupancyPercent}%</strong> {t('pricingManager.occupied')} {formatAdjustment(tier.adjustmentType, tier.adjustmentValue)}
              </span>
              <button type="button" className="campaign-back" onClick={() => startEdit(tier)}>{t('pricingManager.edit')}</button>
              <button type="button" className="room-form-actions-delete room-form-actions-btn" onClick={() => handleDelete(tier.id)}>{t('pricingManager.delete')}</button>
            </>
          )}
        </div>
      ))}
      {!loading && tiers.length === 0 && <p className="muted small">{t('pricingManager.noTiersYet')}</p>}

      <form onSubmit={handleAdd} style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap', marginTop: 8 }}>
        <span>{t('pricingManager.at')}</span>
        <input
          type="number" min={1} max={100} placeholder="e.g. 80" style={{ width: 70 }}
          value={form.minOccupancyPercent}
          onChange={(e) => setForm((f) => ({ ...f, minOccupancyPercent: e.target.value }))}
        />
        <span>{t('pricingManager.occupied')}</span>
        <AdjustmentFields form={form} setForm={setForm} />
        <button type="submit" className="campaign-next" disabled={saving}>{t('pricingManager.add')}</button>
      </form>
    </div>
  );
}

export default function HotelPricingManager({ hotelId }) {
  return (
    <>
      <SeasonalRulesSection hotelId={hotelId} />
      <OccupancyTiersSection hotelId={hotelId} />
    </>
  );
}
