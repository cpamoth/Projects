<template>
  <PageWrapper>
    <div class="about">
      <h1 class="green">About Amoth Consulting</h1>
      <br />
      <section class="company">
        <h2 class="green">Who We Are</h2>
        <br />
        <p>
          Amoth Consulting provides expert business and technology consulting to
          small and medium-sized organizations. We help clients modernize systems,
          improve processes, and deliver measurable results.
        </p>
        <br />
      </section>

      <section class="company">
        <h2 class="green">Our Experience</h2>
        <br />
        <p>
          With over a decade of experience across enterprise, startup, and government sectors, our team brings deep expertise in digital transformation, cloud architecture, and organizational change management. We've successfully delivered projects ranging from strategic consulting engagements to large-scale technology implementations for organizations of all sizes.
        </p>
        <br />
        <p>
          Our consultants hold advanced certifications in cloud platforms, agile methodologies, and security compliance frameworks. We're committed to staying at the forefront of technology trends to deliver cutting-edge solutions tailored to your specific business needs.
        </p>
        <br />
      </section>

      <section class="company">
        <h2 class="green">Why Choose Amoth Consulting?</h2>
        <br />
        <div class="features-grid">
          <div class="feature-card">
            <h3>Expert Team</h3>
            <p>Our consultants bring decades of combined experience across enterprise and startup environments.</p>
          </div>
          <div class="feature-card">
            <h3>Proven Results</h3>
            <p>We deliver measurable impact with a track record of successful digital transformations.</p>
          </div>
          <div class="feature-card">
            <h3>Strategic Focus</h3>
            <p>We align technology initiatives with your business objectives for sustainable growth.</p>
          </div>
          <div class="feature-card">
            <h3>Hands-On Support</h3>
            <p>Our team works closely with yours to ensure successful implementation and knowledge transfer.</p>
          </div>
        </div>
        <br />
      </section>

      <section class="contact">
        <h2 class="green">Contact Us</h2>
        <br />
        <p>If you'd like to work together, reach out by email or phone — or use the contact form below.</p>
        <br/>
        <div class="contact-grid">
          <div class="contact-card">
            <strong>Email:</strong>
            <p><a href="mailto:request@amoth-consulting.com">request@amoth-consulting.com</a></p>
          </div>
          <div class="contact-card">
            <strong>Phone:</strong>
            <p><a href="tel:+18039171788">+1 (803) 917-1788</a></p>
          </div>
          <div class="contact-card">
            <strong>Address:</strong>
            <p>PO Box 123456<br/>Elgin, SC 29045</p>
          </div>
        </div>
        <br/>

        <form class="contact-form" @submit.prevent="sendMessage" aria-label="Contact form">
          <label for="name">
            <span class="label-text">Name <span class="required">*</span></span>
            <input id="name" v-model="name" type="text" required />
          </label>
          <label for="contact-email">
            <span class="label-text">Email <span class="required">*</span></span>
            <input id="contact-email" v-model="email" type="email" required />
          </label>
          <label for="message">
            <span class="label-text">Message <span class="required">*</span></span>
            <textarea id="message" v-model="message" rows="4" required></textarea>
          </label>
          <button :disabled="loading">{{ loading ? 'Sending…' : 'Send Message' }}</button>
          <p v-if="sent" class="sent" role="alert">Thanks — your message was sent (mock).</p>
        </form>
      </section>
    </div>
  </PageWrapper>
</template>

<script setup>
import { ref } from 'vue'

const name = ref('')
const email = ref('')
const message = ref('')
const loading = ref(false)
const sent = ref(false)

function sendMessage() {
  if (!name.value || !email.value || !message.value) return
  loading.value = true
  setTimeout(() => {
    loading.value = false
    sent.value = true
    name.value = ''
    email.value = ''
    message.value = ''
    setTimeout(() => (sent.value = false), 3500)
  }, 700)
}
</script>

<style scoped>
.about { max-width: 900px; margin: 2rem auto; padding: 1rem }
.company p { margin-bottom: .75rem }
.company ul { margin: .5rem 0 1rem 1.25rem }
.services-list { text-align: center }
.services-list p { margin: .5rem 0 }
.contact h2 { text-align: center }
.contact > p { text-align: center }
.contact-grid { display: grid; grid-template-columns: repeat(auto-fit,minmax(200px,1fr)); gap: 1rem; margin-bottom: 1rem }
.contact-card { background: var(--color-background-soft); color: var(--color-text); padding: .75rem; border-radius: 6px; border: 1px solid var(--color-border); text-align: center }
.contact-card strong { display: block; text-align: center }
.contact-card a { color: inherit }
.contact-form { display:flex; flex-direction:column; gap:.5rem }
.contact-form label { display:flex; flex-direction:column; font-weight:600; text-align: left }
.contact-form input, .contact-form textarea { padding:.5rem; border:1px solid #374151; border-radius:4px; background: transparent; color: inherit }
button { align-self:flex-start; padding:.5rem .75rem; background:#2b6cb0; color:#fff; border:none; border-radius:4px }
.sent { color: #2f855a; margin-top:.5rem }
.features-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1.5rem; margin-bottom: 1rem; }
.feature-card { background: var(--color-background-soft); border: 1px solid var(--color-border); border-radius: 10px; padding: 1.5rem; box-shadow: 0 6px 18px rgba(2,6,23,0.04); text-align: left; }
.feature-card h3 { color: var(--color-heading); margin: 0 0 0.75rem; font-size: 1.1rem; }
.feature-card p { color: var(--color-text); opacity: 0.9; margin: 0; line-height: 1.5; }
.required { color: #e53e3e; }
.label-text { display: block; margin-bottom: .5rem; }
</style>