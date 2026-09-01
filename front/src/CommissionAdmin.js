import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { getCommissionOverview, confirmCommission, rejectCommission } from './services/commission';
import { getPlatformSettings, updatePlatformShamCash, uploadPlatformShamCashQr } from './services/platformSettings';

const money = (n) => `$${Math.round(Number(n) || 0).toLocaleString()}`;

export default function CommissionAdmin() {
  const { t } = useTranslation();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [confirmingId, setConfirmingId] = useState(null);
  const [actingId, setActingId] = useState(null); // { hotelId, action }

  const [wallet, setWallet] = useState('');
  const [qrUrl, setQrUrl] = useState('');
  const [newQr, setNewQr] = useState(null);
  const [savingWallet, setSavingWallet] = useState(false);
  const [walletSaved, setWalletSaved] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [overview, settings] = await Promise.all([getCommissionOverview(), getPlatformSettings()]);
      setData(overview);
      setWallet(settings.shamCashWallet);
      setQrUrl(settings.shamCashQrUrl);
    } catch (err) {
      setError(err.message || t('commission.loadError'));
    } finally {
      setLoading(false);
    }
  }, [t]);

  const saveWallet = async () => {
    setSavingWallet(true);
    setWalletSaved(false);
    try {
      if (newQr) {
        const r = await uploadPlatformShamCashQr(newQr);
        setQrUrl(r?.shamCashQrUrl || qrUrl);
        setNewQr(null);
      }
      await updatePlatformShamCash(wallet.trim());
      setWalletSaved(true);
    } catch (err) {
      alert(err.message || t('commission.walletSaveError'));
    } finally {
      setSavingWallet(false);
    }
  };

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

  const reject = async (hotelId) => {
    if (!window.confirm(t('commission.rejectConfirm'))) return;
    setActingId(hotelId);
    try {
      await rejectCommission(hotelId);
      await load();
    } catch (err) {
      alert(err.message || t('commission.rejectError'));
    } finally {
      setActingId(null);
    }
  };


  if (loading) return <p className="admin-stat-sub">{t('commission.loading')}</p>;
  if (error) return <p className="admin-stat-sub" style={{ color: '#e05555' }}>{error}</p>;

  return (
    <div>
      <div className="section-card" style={{ marginBottom: 16 }}>
        <h3 style={{ margin: '0 0 10px' }}>{t('commission.walletTitle')}</h3>
        <p className="admin-stat-sub" style={{ marginTop: 0 }}>{t('commission.walletHint')}</p>
        <div style={{ display: 'flex', gap: 20, flexWrap: 'wrap', alignItems: 'flex-start' }}>
          <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13, flex: 1, minWidth: 220 }}>
            {t('commission.walletNumber')}
            <input
              value={wallet}
              onChange={(e) => setWallet(e.target.value)}
              placeholder="0912345678"
              style={{ padding: '8px 10px', border: '1px solid #cbd5e1', borderRadius: 8 }}
            />
            <span style={{ fontSize: 13 }}>{t('commission.walletQr')}</span>
            <input type="file" accept="image/*" onChange={(e) => setNewQr(e.target.files?.[0] || null)} />
          </label>
          {(newQr || qrUrl) && (
            <img
              src={newQr ? URL.createObjectURL(newQr) : qrUrl}
              alt="Platform Sham Cash QR"
              style={{ width: 140, height: 140, objectFit: 'contain', border: '1px solid #e2e8f0', borderRadius: 8 }}
            />
          )}
        </div>
        <button type="button" className="cta" disabled={savingWallet} onClick={saveWallet} style={{ marginTop: 10 }}>
          {savingWallet ? t('commission.saving') : t('commission.saveWallet')}
        </button>
        {walletSaved && <span style={{ color: '#166534', fontSize: 13, marginInlineStart: 10 }}>{t('commission.walletSaved')}</span>}
      </div>

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
                <th style={th}>{t('commission.col.sender')}</th>
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
                    {h.awaitingConfirmation > 0 && (h.senderWallet || h.senderName) ? (
                      <>
                        {h.senderName && <div>{h.senderName}</div>}
                        {h.senderWallet && <div className="muted small" style={{ fontFamily: 'monospace' }}>{h.senderWallet}</div>}
                      </>
                    ) : '—'}
                  </td>
                  <td style={td}>
                    {h.awaitingConfirmation > 0 && (
                      <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                        <button type="button" className="cta" disabled={confirmingId === h.hotelId} onClick={() => confirm(h.hotelId)}>
                          {confirmingId === h.hotelId ? t('commission.confirming') : t('commission.confirmReceived')}
                        </button>
                        <button type="button" className="cta" disabled={actingId === h.hotelId} onClick={() => reject(h.hotelId)}>
                          {actingId === h.hotelId ? t('commission.confirming') : t('commission.notReceived')}
                        </button>
                      </div>
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
