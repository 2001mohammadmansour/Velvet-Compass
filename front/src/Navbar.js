import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { getCurrentRole, getCurrentUser, getOwnerProfileSummary, clearAuth } from './services/auth';
import NotificationBell from './NotificationBell';
import ProfileMenu from './ProfileMenu';

const NAV_LINKS = [
  { label: 'Home',                     href: '/' },
  { label: 'Services',                 href: '/services' },
  { label: 'Hotels',                   href: '/hotels' },
  { label: 'Our Partners',             href: '/partners' },
  { label: 'About Us',                 href: '/about' },
];

// CHANGED BY AI (2026-07-13): please review — owners no longer have a separate home page, so
// they use the same Home/nav as everyone else, with Dashboard added as a direct top-level link.
const OWNER_NAV_LINK = { label: 'Dashboard', href: '/owner/dashboard' };

export default function Navbar({ transparent = false }) {
  const navigate = useNavigate();
  const location = useLocation();
  const role = getCurrentRole();
  const isAdmin = role === 'admin';
  const isGuest = role === 'guest';
  // CHANGED BY AI (2026-07-13): please review — this is the actual bug fix. Navbar previously
  // only handled 'admin' and 'guest' roles; a logged-in owner (role 'hotel_owner') fell through
  // to the final else branch and saw Login/Sign Up buttons on every public page (Hotels, About
  // Us, etc.) as if they'd been signed out, even though their session was completely untouched.
  const isOwner = role === 'hotel_owner';
  const currentUser = isGuest ? getCurrentUser() : null;
  const ownerProfile = isOwner ? getOwnerProfileSummary() : null;
  const [solid, setSolid] = useState(!transparent);

  useEffect(() => {
    if (!transparent) {
      setSolid(true);
      return;
    }
    const update = () => {
      const hero = document.querySelector('.hero');
      const heroH = hero ? hero.offsetHeight : 0;
      if (!heroH) { setSolid(true); return; }
      if (window.scrollY === 0) setSolid(true);
      else if (window.scrollY < heroH - 80) setSolid(false);
      else setSolid(true);
    };
    window.addEventListener('scroll', update, { passive: true });
    update();
    return () => window.removeEventListener('scroll', update);
  }, [transparent]);

  const handleSignOut = () => { clearAuth(); navigate('/'); };

  const isActive = (href) =>
    href === '/' ? location.pathname === '/' || location.pathname === '/home' : location.pathname === href;

  const navLinks = isOwner ? [...NAV_LINKS, OWNER_NAV_LINK] : NAV_LINKS;

  return (
    <nav className={`navbar${solid ? ' navbar-solid' : ''}`}>
      <Link className="brand" to="/">Velvet Compass</Link>
      <ul className="nav-links">
        {navLinks.map(link => (
          <li key={link.href}>
            <Link
              to={link.href}
              className={isActive(link.href) ? 'nav-active' : ''}
            >
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
      <div className="auth-buttons">
        {isAdmin ? (
          <>
            {/* CHANGED BY AI (2026-07-13): please review — moved from a fixed floating widget to
                sit inline, left of the account controls. */}
            <NotificationBell inline />
            <Link className="btn login" to="/admin">Admin Dashboard</Link>
            <button type="button" className="btn signup" onClick={handleSignOut}>Sign Out</button>
          </>
        ) : isGuest ? (
          <ProfileMenu
            name={currentUser?.username || 'Guest'}
            subtitle={currentUser?.email || ''}
            links={[
              { to: '/profile', label: 'Edit Profile' },
              { to: '/my-bookings', label: '📖 My Bookings' },
            ]}
            onSignOut={handleSignOut}
          />
        ) : isOwner ? (
          // CHANGED BY AI (2026-07-13): please review — this branch was missing entirely, which
          // was the actual bug: a logged-in owner visiting any public page (Hotels, About Us,
          // etc.) fell through to the Login/Sign Up buttons below as if signed out.
          <ProfileMenu
            name={ownerProfile.username}
            subtitle={ownerProfile.hotelName}
            links={[
              { to: '/profile', label: 'Edit Profile' },
              { to: '/owner/dashboard', label: 'Dashboard' },
              { to: '/owner/hotel-info', label: 'Edit Hotel Info' },
              { to: '/owner/requests', label: 'Hotel Requests' },
            ]}
            onSignOut={handleSignOut}
            triggerLabel="Owner profile"
          />
        ) : (
          <>
            <Link className="btn login" to="/login">Login</Link>
            <Link className="btn signup" to="/signup">Sign Up</Link>
          </>
        )}
      </div>
    </nav>
  );
}
