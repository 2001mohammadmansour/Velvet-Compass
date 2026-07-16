import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import './contact.css';

export default function Contact() {
  const { t } = useTranslation();

  const INFO_ITEMS = [
    { icon: '📧', label: t('contact.infoLabels.email'), value: 'support@velvetcompass.com' },
    { icon: '📞', label: t('contact.infoLabels.phone'), value: '+1-800-555-0123' },
    { icon: '📍', label: t('contact.infoLabels.address'), value: '123 Booking Avenue, Travel City' },
    { icon: '⏰', label: t('contact.infoLabels.supportHours'), value: t('contact.supportHoursValue') },
  ];

  const [form, setForm] = useState({ name: '', email: '', subject: '', message: '' });
  const [sent, setSent] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const els = document.querySelectorAll('[data-reveal]');
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          entry.target.classList.toggle('is-visible', entry.isIntersecting);
        });
      },
      { threshold: 0.1 }
    );
    els.forEach(el => observer.observe(el));
    return () => observer.disconnect();
  }, []);

  const handleChange = (e) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.name || !form.email || !form.message) {
      setError(t('contact.fillRequired'));
      return;
    }
    setError('');
    setSent(true);
  };

  return (
    <div className="cp-page">
      <header className="cp-hero">
        <h1>{t('contact.title')}</h1>
        <p>{t('contact.subtitle')}</p>
      </header>

      <section className="cp-body">
        <div className="cp-form-card" data-reveal>
          <h2>{t('contact.sendMessage')}</h2>

          {sent ? (
            <div className="cp-success">
              <div className="cp-success-icon">✉️</div>
              <h3>{t('contact.messageSent')}</h3>
              <p>{t('contact.thanksMessage', { name: form.name, email: form.email })}</p>
              <button
                className="cp-reset-btn"
                onClick={() => {
                  setSent(false);
                  setForm({ name: '', email: '', subject: '', message: '' });
                }}
              >
                {t('contact.sendAnother')}
              </button>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="cp-form">
              <div className="cp-field">
                <label htmlFor="cp-name">{t('contact.fullName')}</label>
                <input
                  id="cp-name"
                  name="name"
                  placeholder={t('contact.fullNamePlaceholder')}
                  value={form.name}
                  onChange={handleChange}
                />
              </div>
              <div className="cp-field">
                <label htmlFor="cp-email">{t('contact.emailAddress')}</label>
                <input
                  id="cp-email"
                  name="email"
                  type="email"
                  placeholder={t('contact.emailPlaceholder')}
                  value={form.email}
                  onChange={handleChange}
                />
              </div>
              <div className="cp-field">
                <label htmlFor="cp-subject">{t('contact.subject')}</label>
                <input
                  id="cp-subject"
                  name="subject"
                  placeholder={t('contact.subjectPlaceholder')}
                  value={form.subject}
                  onChange={handleChange}
                />
              </div>
              <div className="cp-field">
                <label htmlFor="cp-message">{t('contact.message')}</label>
                <textarea
                  id="cp-message"
                  name="message"
                  rows={6}
                  placeholder={t('contact.messagePlaceholder')}
                  value={form.message}
                  onChange={handleChange}
                />
              </div>
              {error && <p className="cp-error">{error}</p>}
              <button type="submit" className="cp-submit-btn">{t('contact.sendMessageBtn')}</button>
            </form>
          )}
        </div>

        <div className="cp-info">
          {INFO_ITEMS.map((item, i) => (
            <div
              key={item.label}
              className="cp-info-card"
              data-reveal="right"
              style={{ transitionDelay: `${i * 0.1}s` }}
            >
              <span className="cp-info-icon">{item.icon}</span>
              <div>
                <h4>{item.label}</h4>
                <p>{item.value}</p>
              </div>
            </div>
          ))}
        </div>
      </section>

      <footer className="cp-footer">
        <p>{t('contact.footer')}</p>
      </footer>
    </div>
  );
}
