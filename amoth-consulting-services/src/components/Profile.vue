<template>
  <PageWrapper>
    <div class="profile">
      <h1 class="green">My Profile</h1>
      <br />
      <section class="profile-section">
        <form @submit.prevent="updateProfile" class="profile-form">
          <div class="field">
            <label for="username"><span class="label-text">Username <span class="required">*</span></span></label>
            <input id="username" v-model="username" type="text" disabled />
            <p class="info">Username cannot be changed</p>
          </div>

          <div class="field">
            <label for="email"><span class="label-text">Email</span></label>
            <input id="email" v-model="email" type="email" />
          </div>

          <div class="field">
            <label for="fullname"><span class="label-text">Full Name</span></label>
            <input id="fullname" v-model="fullName" type="text" />
          </div>

          <div class="field">
            <label for="bio"><span class="label-text">Bio</span></label>
            <textarea id="bio" v-model="bio" rows="4"></textarea>
          </div>

          <button type="submit" :disabled="saving">{{ saving ? 'Saving…' : 'Save Changes' }}</button>
          <p v-if="saved" class="success" role="alert">Profile updated successfully!</p>
        </form>
      </section>

      <section class="profile-section">
        <h2>Change Password</h2>
        <br />
        <form @submit.prevent="changePassword" class="profile-form">
          <div class="field">
            <label for="currentPassword"><span class="label-text">Current Password <span class="required">*</span></span></label>
            <input id="currentPassword" v-model="currentPassword" type="password" required />
          </div>

          <div class="field">
            <label for="newPassword"><span class="label-text">New Password <span class="required">*</span></span></label>
            <input id="newPassword" v-model="newPassword" type="password" minlength="12" required />
            <p v-if="newPassword.length < 12" class="error">Password must be at least 12 characters</p>
          </div>

          <div class="field">
            <label for="confirmPassword"><span class="label-text">Confirm New Password <span class="required">*</span></span></label>
            <input id="confirmPassword" v-model="confirmPassword" type="password" minlength="6" required />
          </div>

          <button type="submit" :disabled="savingPassword || newPassword.length < 12">{{ savingPassword ? 'Updating…' : 'Update Password' }}</button>
          <p v-if="passwordError" class="error" role="alert">{{ passwordError }}</p>
          <p v-if="passwordChanged" class="success" role="alert">Password changed successfully!</p>
        </form>
      </section>

      <BillingOptions />
    </div>
  </PageWrapper>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import BillingOptions from '../features/profile/components/BillingOptions.vue'
import { changePassword as changePasswordApi, getAuthUser } from '../services/authApi.js'

const router = useRouter()
const username = ref('')
const email = ref('')
const fullName = ref('')
const bio = ref('')
const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')

const saving = ref(false)
const saved = ref(false)
const savingPassword = ref(false)
const passwordChanged = ref(false)
const passwordError = ref('')

onMounted(() => {
  const user = getAuthUser()
  if (!user) {
    router.push('/login')
    return
  }
  username.value = user.username || ''

  // Load profile data from localStorage
  const profileData = sessionStorage.getItem('profileDraft')
  if (profileData) {
    try {
      const profile = JSON.parse(profileData)
      email.value = profile.email || ''
      fullName.value = profile.fullName?.trim() || username.value
      bio.value = profile.bio || ''
    } catch (e) {
      console.error('Error loading profile data', e)
    }
  }
})

function updateProfile() {
  saving.value = true
  saved.value = false
  
  setTimeout(() => {
    saving.value = false
    const profileData = {
      email: email.value,
      fullName: fullName.value?.trim() || username.value,
      bio: bio.value
    }
    sessionStorage.setItem('profileDraft', JSON.stringify(profileData))
    window.dispatchEvent(new Event('authStateChanged'))
    saved.value = true
    setTimeout(() => {
      saved.value = false
    }, 3000)
  }, 600)
}

async function changePassword() {
  passwordError.value = ''
  passwordChanged.value = false

  // Validate passwords match
  if (newPassword.value !== confirmPassword.value) {
    passwordError.value = 'New passwords do not match'
    return
  }

  // Validate password length
  if (newPassword.value.length < 12) {
    passwordError.value = 'Password must be at least 12 characters'
    return
  }

  savingPassword.value = true
  try {
    await changePasswordApi({ currentPassword: currentPassword.value, newPassword: newPassword.value, confirmPassword: confirmPassword.value })
    currentPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
    passwordChanged.value = true
  } catch (error) {
    passwordError.value = error.message
  } finally {
    savingPassword.value = false
  }
}
</script>

<style scoped>
.profile { width: 100%; margin: 0; padding: .5rem }
.profile h1 { margin-bottom: 1rem }

.profile-section { margin-bottom: 3rem; padding: 1.5rem; border: 1px solid var(--color-border); border-radius: 8px; background: var(--color-background-soft) }
.profile-section h2 { color: var(--color-heading); margin-bottom: 1rem; font-size: 1.3rem }

.profile-form { display: flex; flex-direction: column; gap: 1rem }
.field { display: flex; flex-direction: column }
label { margin-bottom: .5rem; font-weight: 600; color: var(--color-text) }
.label-text { display: block; margin-bottom: .5rem }
input, textarea { padding: .75rem; border: 1px solid var(--color-border); border-radius: 4px; background: transparent; color: var(--color-text); font-family: inherit }
input:disabled { opacity: 0.6; cursor: not-allowed }
input::placeholder, textarea::placeholder { color: rgba(145,145,145,0.5) }

button { align-self: flex-start; padding: .6rem 1rem; border: none; background: var(--vt-c-indigo); color: #fff; border-radius: 4px; cursor: pointer; font-weight: 600 }
button:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(2,6,23,0.06); transition: all .18s ease }
button:disabled { opacity: .6; cursor: not-allowed }

.info { color: var(--color-text); font-size: .85rem; opacity: 0.8; margin-top: .25rem }
.error { color: #e53e3e; font-size: .85rem; margin-top: .25rem }
.success { color: #2f855a; font-size: .85rem; margin-top: .5rem }
.required { color: #e53e3e }
</style>
