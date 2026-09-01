import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { getCommissionOverview, confirmCommission } from './services/commission';

const money = (n) => `$${Math.round(Number(n) || 0).toLocaleString()}`;

export default function CommissionAdmin() {
  const { t } = useTranslation();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [confirmingId, setConfirmingId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      setData(await getCommissionOverview());
    } catch (err) {
      setError(err.message || t('commission.loadError'));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => { load(); }, [load]);

  const confirm = async (hotelId) => {
    setConfirmingId(hotelId);
    try {
      await confirmCommission(hotelId);
      await load();
    } catch (err) {
      alert(err.message || t('commission.confirmError'));
    } finally {
      setConfirmingId(null);
    }
  };

  if (loading) return <p className="admin-stat-sub">{t('commission.loading')}</p>;
  if (error) return <p className="admin-stat-sub" style={{ color: '#e05555' }}>{error}</p>;

  return (
    <div>
      <div className="admin-stats-row" style={{ marginBottom: 16 }}>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('commission.pending')}</div>
          <div className="admin-stat-value" style={{ fontSize: 20 }}>{money(data.pendingTotal)}</div>
          <div className="admin-stat-sub">{t('commission.pendingSub')}</div>
        </div>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('commission.collected')}</div>
          <div className="admin-stat-value" style={{ fontSize: 20 }}>{money(data.collectedTotal)}</div>
          <div className="admin-stat-sub">{t('commission.collectedSub')}</div>
        </div>
      </div>

      {data.hotels.length === 0 ? (
        <p className="admin-stat-sub">{t('commission.nothingOwed')}</p>
      ) : (
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={th}>{t('commission.col.hotel')}</th>
                <th style={th}>{t('commission.col.owed')}</th>
                <th style={th}>{t('commission.col.claimed')}</th>
                <th style={th}></th>
              </tr>
            </thead>
            <tbody>
              {data.hotels.map((h) => (
                <tr key={h.hotelId}>
                  <td style={td}>{h.hotelName}<div className="muted small">{h.ownerName}</div></td>
                  <td style={td}>{money(h.owed)}</td>
                  <td style={td}>{h.awaitingConfirmation > 0 ? money(h.awaitingConfirmation) : '—'}</td>
                  <td style={td}>
                    {h.awaitingConfirmation > 0 && (
                      <button type="button" className="cta" disabled={confirmingId === h.hotelId} onClick={() => confirm(h.hotelId)}>
                        {confirmingId === h.hotelId ? t('commission.confirming') : t('commission.confirmReceived')}
                      </button>
                    )}
                  </td>
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
