<template>
  <PageWrapper>
    <section class="services">
      <div class="container">
        <h1>Our Services</h1>
        <p class="intro">We partner with organizations to deliver practical, measurable outcomes across technology, product, and operations.</p>

        <div class="cards">
          <article class="card">
            <h3>Strategy & Transformation</h3>
            <p>Roadmapping, digital strategy, operating-model change, and program delivery to align technology with business goals.</p>
          </article>

          <article class="card">
            <h3>Custom Development</h3>
            <p>Full-stack web and mobile applications, system integrations, and API-first architectures built for scale and maintainability.</p>
          </article>

          <article class="card">
            <h3>Cloud, DevOps & Infrastructure</h3>
            <p>Cloud migration, CI/CD, infrastructure as code, and platform engineering to improve reliability and delivery speed.</p>
          </article>

          <article class="card">
            <h3>Security & Compliance</h3>
            <p>Risk assessments, secure design reviews, compliance readiness, and managed security practices to protect your business.</p>
          </article>

          <article class="card">
            <h3>Data & Analytics</h3>
            <p>Data strategy, pipelines, analytics, and ML enablement to turn data into trusted insights and products.</p>
          </article>

          <article class="card">
            <h3>UX, Product & Design</h3>
            <p>Product discovery, UX research, and design systems that deliver delightful, usable experiences for customers.</p>
          </article>
        </div>

      </div>
    </section>
  </PageWrapper>
</template>

<style scoped>
.services { max-width: 1100px; margin: 0 auto; padding: 2rem 1rem; }
.container { text-align: center; }
.intro { margin-top: .5rem; color: var(--color-text); opacity: .92 }
.cards { margin-top: 1.25rem; display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1rem; }
.card { background: var(--color-background-soft); border: 1px solid var(--color-border); border-radius: 10px; padding: 1rem; box-shadow: 0 6px 18px rgba(2,6,23,0.04); text-align: left }
.card h3 { margin: 0 0 .5rem; color: var(--color-heading); font-size: 1.05rem }
.card p { margin: 0; color: var(--color-text); opacity: .9; font-size: .95rem }

@media (min-width: 1024px) {
  .container { text-align: left }
  .services { padding: 2rem 0 }
}
</style>
