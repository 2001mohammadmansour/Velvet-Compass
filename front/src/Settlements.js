import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { getSettlementPreview, runSettlement, getSettlementHistory } from './services/settlements';

const money = (n) => `$${Math.round(Number(n) || 0).toLocaleString()}`;

export default function Settlements() {
  const { t } = useTranslation();
  const [preview, setPreview] = useState([]);
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [runningId, setRunningId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [p, h] = await Promise.all([getSettlementPreview(), getSettlementHistory()]);
      setPreview(p);
      setHistory(h);
    } catch (err) {
      setError(err.message || t('settlements.loadError'));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => { load(); }, [load]);

  const settle = async (hotelId) => {
    setRunningId(hotelId);
    try {
      await runSettlement(hotelId);
      await load();
    } catch (err) {
      alert(err.message || t('settlements.runError'));
    } finally {
      setRunningId(null);
    }
  };

  const settleAll = async () => {
    if (!window.confirm(t('settlements.settleAllConfirm', { count: preview.length }))) return;
    for (const row of preview) {
      // eslint-disable-next-line no-await-in-loop
      try { await runSettlement(row.hotelId); } catch { /* keep going */ }
    }
    await load();
  };

  const dirLabel = (d) => t(`settlements.direction.${d}`);

  if (loading) return <p className="admin-stat-sub">{t('settlements.loading')}</p>;
  if (error) return <p className="admin-stat-sub" style={{ color: '#e05555' }}>{error}</p>;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
        <h3 style={{ margin: 0 }}>{t('settlements.pendingTitle')}</h3>
        {preview.length > 0 && (
          <button type="button" className="cta" onClick={settleAll}>{t('settlements.settleAll')}</button>
        )}
      </div>

      {preview.length === 0 ? (
        <p className="admin-stat-sub">{t('settlements.nothingDue')}</p>
      ) : (
        <div style={{ overflowX: 'auto' }}>
          <table className="admin-table" style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={th}>{t('settlements.col.hotel')}</th>
                <th style={th}>{t('settlements.col.bookings')}</th>
                <th style={th}>{t('settlements.col.ownerCredit')}</th>
                <th style={th}>{t('settlements.col.commission')}</th>
                <th style={th}>{t('settlements.col.clawback')}</th>
                <th style={th}>{t('settlements.col.transfer')}</th>
                <th style={th}></th>
              </tr>
            </thead>
            <tbody>
              {preview.map((r) => (
                <tr key={r.hotelId}>
                  <td style={td}>{r.hotelName}<div className="muted small">{r.ownerName}</div></td>
                  <td style={td}>{r.bookingCount}</td>
                  <td style={td}>{money(r.ownerCredit)}</td>
                  <td style={td}>{money(r.platformCommission)}</td>
                  <td style={td}>{r.clawbackAmount ? `− ${money(r.clawbackAmount)}` : '—'}</td>
                  <td style={td}>
                    <strong>{money(r.netAmount)}</strong>
                    <div className="muted small">{dirLabel(r.direction)}</div>
                  </td>
                  <td style={td}>
                    <button type="button" className="cta" disabled={runningId === r.hotelId} onClick={() => settle(r.hotelId)}>
                      {runningId === r.hotelId ? t('settlements.settling') : t('settlements.settle')}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <h3 style={{ marginTop: 28 }}>{t('settlements.historyTitle')}</h3>
      {history.length === 0 ? (
        <p className="admin-stat-sub">{t('settlements.noHistory')}</p>
      ) : (
        <div style={{ overflowX: 'auto' }}>
          <table className="admin-table" style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={th}>{t('settlements.col.period')}</th>
                <th style={th}>{t('settlements.col.hotel')}</th>
                <th style={th}>{t('settlements.col.bookings')}</th>
                <th style={th}>{t('settlements.col.transfer')}</th>
              </tr>
            </thead>
            <tbody>
              {history.map((s) => (
                <tr key={s.id}>
                  <td style={td}>{s.periodLabel}</td>
                  <td style={td}>{s.hotelName}</td>
                  <td style={td}>{s.bookingCount}</td>
                  <td style={td}>{money(s.netAmount)} <span className="muted small">{dirLabel(s.direction)}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

const th = { textAlign: 'left', padding: '8px 10px', borderBottom: '2px solid #e2e8f0', fontSize: 13 };
const td = { padding: '8px 10px', borderBottom: '1px solid #f1f5f9', fontSize: 14, verticalAlign: 'top' };
