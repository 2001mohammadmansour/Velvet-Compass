import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { getStats } from "./services/hotels";
import "./services.css";

const SERVICES = [
  {
    id: 1,
    icon: "⚡",
    title: "Frictionless Booking",
    desc: "Choose your room in seconds with a clear, modern interface and instant confirmation.",
    tag: "Fastest flow",
    color: "#2A3D66",
  },
  {
    id: 2,
    icon: "🛡️",
    title: "Trusted Hotel Quality",
    desc: "Every property is validated with care so guests enjoy a reliable stay every time.",
    tag: "Verified stays",
    color: "#6C8BC7",
  },
  {
    id: 3,
    icon: "🌐",
    title: "Real Guest Reviews",
    desc: "Genuine feedback from real travelers helps you choose the ideal hotel with confidence.",
    tag: "Authentic voices",
    color: "#2A3D66",
  },
  {
    id: 4,
    icon: "⏱️",
    title: "24/7 Support",
    desc: "Our team is ready around the clock to assist with bookings, changes, and questions.",
    tag: "Always available",
    color: "#6C8BC7",
  },
  {
    id: 5,
    icon: "💎",
    title: "Exclusive Deals",
    desc: "Enjoy competitive rates, special promotions, and curated offers for loyal travelers.",
    tag: "Premium value",
    color: "#E8B86D",
  },
  {
    id: 6,
    icon: "🔒",
    title: "Secure Payments",
    desc: "Payments are protected with advanced encryption and privacy-first handling.",
    tag: "Safe checkout",
    color: "#2A3D66",
  },
  {
    id: 7,
    icon: "🗓️",
    title: "Free Cancellation",
    desc: "Change your mind? Most rooms offer a free cancellation window before check-in.",
    tag: "Peace of mind",
    color: "#E8B86D",
  },
];

function ServiceCard({ service, index }) {
  const [visible, setVisible] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) setTimeout(() => setVisible(true), index * 80);
      },
      { threshold: 0.1 }
    );
    if (ref.current) observer.observe(ref.current);
    return () => observer.disconnect();
  }, [index]);

  return (
    <div
      ref={ref}
      className={`svc-card ${visible ? "svc-visible" : ""}`}
      style={{ "--svc-card-color": service.color }}
    >
      <div className="svc-card-bar" />
      <div className="svc-card-icon">{service.icon}</div>
      <span className="svc-card-tag">{service.tag}</span>
      <h3 className="svc-card-title">{service.title}</h3>
      <p className="svc-card-desc">{service.desc}</p>
    </div>
  );
}

export default function ServicesSection() {
  const [visible, setVisible] = useState(false);
  const [stats, setStats] = useState({ hotels: 0, bookings: 0, cities: 0 });
  const navigate = useNavigate();

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 100);
    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    getStats().then((s) => setStats(s)).catch(() => {});
  }, []);

  const visibilityClass = visible ? "svc-visible" : "svc-hidden";

  const displayStats = [
    { n: stats.hotels.toLocaleString(), l: "Verified hotels" },
    { n: stats.bookings.toLocaleString(), l: "Successful bookings" },
    { n: stats.cities.toLocaleString(), l: "Cities covered" },
    { n: "24/7", l: "Support coverage" },
  ];

  return (
    <section className="svc-sec">
      <div className="svc-container">
        <div className={`svc-header ${visibilityClass}`}>
          <div className="svc-badge">Signature Services</div>
          <h2 className="svc-title">Everything your stay deserves in one place</h2>
          <p className="svc-sub">Built to be effortless, elegant, and dependable — from discovery to checkout.</p>
        </div>

        <div className={`svc-stats-bar ${visibilityClass}`}>
          {displayStats.map((stat, i) => (
            <div className="svc-stat" key={stat.l}>
              <div>
                <div className="svc-stat-n">{stat.n}</div>
                <div className="svc-stat-l">{stat.l}</div>
              </div>
              {i < displayStats.length - 1 && <div className="svc-stat-divider" />}
            </div>
          ))}
        </div>

        <div className="svc-grid-3">
          {SERVICES.slice(0, 3).map((service, i) => (
            <ServiceCard key={service.id} service={service} index={i} />
          ))}
        </div>

        <div className="svc-grid-4">
          {SERVICES.slice(3).map((service, i) => (
            <ServiceCard key={service.id} service={service} index={i + 3} />
          ))}
        </div>

        <div className={`svc-cta-row ${visibilityClass}`}>
          <button className="svc-btn-primary" onClick={() => navigate("/hotels")}>
            Start booking now
          </button>
        </div>
      </div>
    </section>
  );
}
