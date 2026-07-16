import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getStats } from "./services/hotels";
import "./services.css";

const SERVICE_META = [
  { id: 1, icon: "⚡", key: "booking", color: "#2A3D66" },
  { id: 2, icon: "🛡️", key: "quality", color: "#6C8BC7" },
  { id: 3, icon: "🌐", key: "reviews", color: "#2A3D66" },
  { id: 4, icon: "⏱️", key: "support", color: "#6C8BC7" },
  { id: 5, icon: "💎", key: "deals", color: "#E8B86D" },
  { id: 6, icon: "🔒", key: "payments", color: "#2A3D66" },
  { id: 7, icon: "🗓️", key: "cancellation", color: "#E8B86D" },
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
  const { t } = useTranslation();
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

  const services = SERVICE_META.map((meta) => ({
    ...meta,
    title: t(`services.cards.${meta.key}.title`),
    tag: t(`services.cards.${meta.key}.tag`),
    desc: t(`services.cards.${meta.key}.desc`),
  }));

  const displayStats = [
    { n: stats.hotels.toLocaleString(), l: t('services.statHotels') },
    { n: stats.bookings.toLocaleString(), l: t('services.statBookings') },
    { n: stats.cities.toLocaleString(), l: t('services.statCities') },
    { n: "24/7", l: t('services.statSupport') },
  ];

  return (
    <section className="svc-sec">
      <div className="svc-container">
        <div className={`svc-header ${visibilityClass}`}>
          <div className="svc-badge">{t('services.badge')}</div>
          <h2 className="svc-title">{t('services.title')}</h2>
          <p className="svc-sub">{t('services.subtitle')}</p>
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
          {services.slice(0, 3).map((service, i) => (
            <ServiceCard key={service.id} service={service} index={i} />
          ))}
        </div>

        <div className="svc-grid-4">
          {services.slice(3).map((service, i) => (
            <ServiceCard key={service.id} service={service} index={i + 3} />
          ))}
        </div>

        <div className={`svc-cta-row ${visibilityClass}`}>
          <button className="svc-btn-primary" onClick={() => navigate("/hotels")}>
            {t('services.startBooking')}
          </button>
        </div>
      </div>
    </section>
  );
}
