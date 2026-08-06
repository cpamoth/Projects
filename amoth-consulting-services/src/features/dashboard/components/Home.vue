<script setup>
defineProps({
  msg: {
    type: String,
    required: true,
  },
})
</script>

<template>
  <PageWrapper>
    <section class="greetings">
    <div class="hero">
      <h1 class="green">{{ msg }}</h1>
      <h2 class="subtitle">Welcome to Amoth Consulting</h2>
      <p class="lead">We help organizations modernize technology, streamline operations, and deliver measurable business outcomes.</p>

      <p class="lead-2">Our team combines deep technical expertise with industry knowledge to deliver scalable, secure, and user-centered solutions.</p>

      <p class="lead-3">At Amoth Consulting, we partner with clients across sectors to craft practical strategies, accelerate growth, and build resilient digital experiences that support long-term success.</p>

      <p class="lead-3">This site is built for accessibility, with clear navigation, responsive layouts, and ARIA-friendly controls to support all users.</p>

    </div>

    <div class="ctas">
      <RouterLink to="/about" class="btn">Learn More</RouterLink>
      <RouterLink to="/register" class="btn secondary">Get Started</RouterLink>
    </div>

    <section class="overview">
      <div class="overview-grid">
        <article class="overview-card">
          <h3>Trusted delivery</h3>
          <p>We turn complex initiatives into clear roadmaps with measurable outcomes.</p>
        </article>
        <article class="overview-card">
          <h3>Customer-centered design</h3>
          <p>Every solution is built to improve adoption, performance, and long-term value.</p>
        </article>
        <article class="overview-card">
          <h3>Modern operations</h3>
          <p>We streamline processes, automate workflows, and improve resilience across teams.</p>
        </article>
      </div>
    </section>

    <section class="accessibility-footer">
      <RouterLink to="/accessibility" class="accessibility-link">
        Accessibility statement: this site is designed for inclusive, accessible use with clear navigation and ARIA-friendly controls.
      </RouterLink>
    </section>

    <section class="copyright-footer">
      <span class="copyright-text">
        © 2026 Amoth Consulting. All rights reserved.
      </span>
    </section>
    </section>
  </PageWrapper>
</template>

<style scoped>
.greetings { --accent: hsla(160, 100%, 37%, 1); width: 100%; margin:0; padding:.5rem; text-align: center; }

h1 {
  font-weight: 700;
  font-size: clamp(1.8rem, 4vw, 2.6rem);
  color: var(--color-heading);
  margin: 0 0 0.5rem;
}

.subtitle {
  font-weight: 600;
  font-size: clamp(1.05rem, 2.2vw, 1.5rem);
  color: var(--accent);
  margin: 0.25rem 0 0.75rem;
}

h3 {
  font-size: 1.1rem;
  margin: 0 0 0.5rem;
  color: var(--color-heading);
}

.hero {
  width: 100%;
  margin: 0;
  padding: 1.5rem;
  text-align: center;
  background: var(--color-background-soft);
  border-radius: 10px;
  box-shadow: 0 6px 18px rgba(2,6,23,0.04);
}

.lead { margin-top: .6rem; color: var(--color-text); opacity: .9 }

.lead-2, .lead-3 { margin-top: .6rem; color: var(--color-text); opacity: .9 }

.ctas { margin-top: 1rem; display:flex; gap:.75rem; justify-content:center; flex-wrap:wrap }
.btn { padding:.65rem 1rem; border-radius:8px; text-decoration:none; display:inline-block; background: var(--accent); color:#fff }
.btn.secondary { background: transparent; color: var(--accent); border:1px solid var(--color-border) }

@media (hover: hover) {
  .btn:hover { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(2,6,23,0.06); transition: all .18s ease }
}

.overview { margin-top:1.5rem }
.overview-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  width: 100%;
  margin: 1.5rem 0;
}
.overview-card {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 14px;
  padding: 1.25rem;
  text-align: left;
}
.overview-card h3 {
  margin: 0 0 0.6rem;
  color: var(--color-heading);
}
.overview-card p {
  margin: 0;
  color: var(--color-text);
  opacity: 0.86;
  line-height: 1.7;
}

.accessibility-footer {
  margin-top: 1.5rem;
  text-align: center;
}
.accessibility-link {
  display: inline-block;
  color: var(--accent);
  text-decoration: none;
  font-weight: 600;
  border-bottom: 1px dashed var(--accent);
  padding-bottom: 0.1rem;
}
.accessibility-link:hover,
.accessibility-link:focus-visible {
  text-decoration: underline;
}

.copyright-footer {
  margin-top: 1rem;
  text-align: center;
}
.copyright-link {
  color: var(--color-text);
  text-decoration: none;
  opacity: 0.76;
  font-size: 0.95rem;
}
.copyright-link:hover,
.copyright-link:focus-visible {
  text-decoration: underline;
  opacity: 1;
}

@media (max-width: 768px) {
  .overview-grid {
    grid-template-columns: 1fr;
  }
  .overview-card {
    text-align: center;
    padding: 1rem;
  }
  .greetings { padding: 1.25rem 0; }
  .hero { padding: 1.25rem; }
  h1 { font-size: clamp(1.5rem, 6vw, 2.2rem); }
  .subtitle { font-size: clamp(0.95rem, 4vw, 1.2rem); }
  .lead,
  .lead-2,
  .lead-3 { margin-top: 0.55rem; }
  .ctas { gap: 0.6rem; }
  .btn { width: 100%; padding: 0.75rem 1rem; text-align: center; }
  .btn.secondary { width: 100%; }
}

@media (min-width: 1024px) {
  .greetings { width: 100%; margin:0; padding:2rem 1rem }
  .hero { padding:2rem; text-align:center }
  .ctas { justify-content:center }
  .overview { margin-top:1rem; text-align:center }
  .overview ul { margin:.5rem auto }
}
</style>
