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

    </div>

    <div class="ctas">
      <RouterLink to="/about" class="btn">Learn More</RouterLink>
      <RouterLink to="/register" class="btn secondary">Get Started</RouterLink>
    </div>
    </section>
  </PageWrapper>
</template>

<style scoped>
.greetings { --accent: hsla(160, 100%, 37%, 1); max-width:1100px; margin:0 auto; padding:2rem 1rem; text-align: center; }

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
  max-width: 880px;
  margin: 0 auto;
  padding: 1.5rem;
  text-align: center;
  background: var(--color-background-soft);
  border-radius: 10px;
  box-shadow: 0 6px 18px rgba(2,6,23,0.04);
}

.lead { margin-top: .6rem; color: var(--color-text); opacity: .9 }

.lead-2 { margin-top: .6rem; color: var(--color-text); opacity: .9 }

.ctas { margin-top: 1rem; display:flex; gap:.75rem; justify-content:center; flex-wrap:wrap }
.btn { padding:.65rem 1rem; border-radius:8px; text-decoration:none; display:inline-block; background: var(--accent); color:#fff }
.btn.secondary { background: transparent; color: var(--accent); border:1px solid var(--color-border) }

@media (hover: hover) {
  .btn:hover { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(2,6,23,0.06); transition: all .18s ease }
}

.overview { margin-top:1.5rem }
.overview ul { list-style: disc; margin:.5rem auto; max-width:640px; padding-left:0; text-align:center; display:inline-block; list-style-position: inside }

@media (min-width: 1024px) {
  .greetings { max-width:1100px; margin:0 auto; padding:2rem 0 }
  .hero { padding:2rem; text-align:center }
  .ctas { justify-content:center }
  .overview { margin-top:1rem; text-align:center }
  .overview ul { margin:.5rem auto }
}
</style>
