<template>
  <PageWrapper>
    <section class="accessibility-page">
      <div class="panel">
        <h1>Accessibility Statement</h1>
        <p>Amoth Consulting is committed to building an accessible and inclusive website experience. This site has been designed so that users with diverse abilities can navigate, understand, and interact with the content.</p>

        <h2>What we have done</h2>
        <ul>
          <li>Clear, semantic structure for headings and page content.</li>
          <li>Keyboard-friendly navigation and focus-visible indicators.</li>
          <li>Responsive layouts that work on desktop, tablet, and mobile devices.</li>
          <li>ARIA labels and accessible controls for buttons, links, and navigation.</li>
          <li>Readable text contrast and accessible interaction feedback.</li>
        </ul>

        <h2>How to use this site</h2>
        <p>Use the main navigation to move between pages, and rely on keyboard focus cues for interactive elements. If you use assistive technology, this site supports screen readers and clear landmark navigation.</p>

        <h2>Want to learn more?</h2>
        <p>If you have questions about accessibility or need support, please contact us through the About page.</p>
      </div>
    </section>
  </PageWrapper>
</template>

<script setup>
// accessibility page component
</script>

<style scoped>
.accessibility-page {
  padding: .5rem;
  width: 100%;
  margin: 0;
}

.panel {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 16px;
  padding: 2rem;
  box-shadow: 0 8px 24px rgba(2,6,23,0.08);
}

h1,
h2 {
  color: var(--color-heading);
  margin-top: 0;
}

h1 {
  font-size: clamp(2rem, 4vw, 2.6rem);
  margin-bottom: 1rem;
}

h2 {
  font-size: 1.2rem;
  margin-top: 1.75rem;
  margin-bottom: 0.75rem;
}

p,
ul {
  color: var(--color-text);
  line-height: 1.75;
  margin: 0;
}

ul {
  margin-top: 1rem;
  padding-left: 1.25rem;
}

li {
  margin-bottom: 0.75rem;
}

@media (max-width: 768px) {
  .accessibility-page {
    padding: 1.5rem 0.75rem;
  }

  .panel {
    padding: 1.5rem;
  }
}
</style>
