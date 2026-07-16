import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './notificationBell.css';
import { getCurrentUser, getCurrentRole } from './services/auth';
import { getMyNotifications, getUnreadCount, markNotificationRead, markAllNotificationsRead } from './services/notifications';

const POLL_MS = 45000;

function timeAgo(dateString, t) {
  const seconds = Math.floor((Date.now() - new Date(dateString).getTime()) / 1000);
  if (seconds < 60) return t('notifications.justNow');
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return t('notifications.minutesAgo', { count: minutes });
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return t('notifications.hoursAgo', { count: hours });
  return t('notifications.daysAgo', { count: Math.floor(hours / 24) });
}

// CHANGED BY AI (2026-07-13): please review — now supports an `inline` mode so it can sit
// directly next to the profile icon in Navbar.js instead of always floating. Floating mode is
// kept as a fallback for owner/admin dashboard pages, which don't render Navbar at all.
export default function NotificationBell({ inline = false }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const user = getCurrentUser();
  const [open, setOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(false);
  const panelRef = useRef(null);

  useEffect(() => {
    if (!user?.id) return;
    let mounted = true;
    const poll = () => {
      getUnreadCount().then((count) => { if (mounted) setUnreadCount(count); }).catch(() => {});
    };
    poll();
    const interval = setInterval(poll, POLL_MS);
    return () => { mounted = false; clearInterval(interval); };
  }, [user?.id]);

  useEffect(() => {
    if (!open) return;
    function handleClickOutside(e) {
      if (panelRef.current && !panelRef.current.contains(e.target)) setOpen(false);
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [open]);

  if (!user?.id) return null;

  const toggleOpen = () => {
    const next = !open;
    setOpen(next);
    if (next) {
      setLoading(true);
      getMyNotifications()
        .then(setNotifications)
        .catch(() => setNotifications([]))
        .finally(() => setLoading(false));
    }
  };

  const handleItemClick = async (n) => {
    if (!n.isRead) {
      setNotifications((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
      setUnreadCount((c) => Math.max(0, c - 1));
      markNotificationRead(n.id).catch(() => {});
    }
    setOpen(false);
    const isOwner = getCurrentRole() === 'hotel_owner';
    if (n.relatedHotelRequestId) {
      navigate('/owner/requests');
    } else if (n.relatedBookingId) {
      navigate(isOwner ? '/owner/dashboard' : '/my-bookings');
    }
  };

  const handleMarkAllRead = async () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
    setUnreadCount(0);
    markAllNotificationsRead().catch(() => {});
  };

  return (
    <div className={inline ? 'nb-root-inline' : 'nb-root'} ref={panelRef}>
      <button className={`nb-bell ${inline ? 'nb-bell-inline' : ''}`} onClick={toggleOpen} aria-label={t('notifications.ariaLabel')}>
        🔔
        {unreadCount > 0 && <span className="nb-badge">{unreadCount > 9 ? '9+' : unreadCount}</span>}
      </button>

      {open && (
        <div className={`nb-panel ${inline ? 'nb-panel-inline' : ''}`}>
          <div className="nb-panel-header">
            <span>{t('notifications.title')}</span>
            {notifications.some((n) => !n.isRead) && (
              <button className="nb-mark-all" onClick={handleMarkAllRead}>{t('notifications.markAllRead')}</button>
            )}
          </div>
          <div className="nb-list">
            {loading && <p className="nb-hint">{t('notifications.loading')}</p>}
            {!loading && notifications.length === 0 && <p className="nb-hint">{t('notifications.noneYet')}</p>}
            {!loading && notifications.map((n) => (
              <div
                key={n.id}
                className={`nb-item ${n.isRead ? '' : 'nb-item-unread'}`}
                onClick={() => handleItemClick(n)}
              >
                <div className="nb-item-title">{n.title}</div>
                <div className="nb-item-message">{n.message}</div>
                <div className="nb-item-time">{timeAgo(n.createdAt, t)}</div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
