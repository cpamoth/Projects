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

          <button type="submit" :disabled="saving || loadingProfile">{{ saving ? 'Saving…' : 'Save Changes' }}</button>
          <p v-if="loadingProfile" role="status">Loading profile…</p>
          <p v-if="profileError" class="error" role="alert">{{ profileError }}</p>
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

      <section class="profile-section">
        <h2>Multi-factor authentication</h2>
        <p>Protect your account with an authenticator app.</p>
        <button type="button" :disabled="settingUpMfa" @click="startMfaSetup">{{ settingUpMfa ? 'Preparing…' : 'Set up MFA' }}</button>
        <p v-if="mfaUri" class="mfa-uri">{{ mfaUri }}</p>
        <form v-if="mfaUri" @submit.prevent="confirmMfaSetup" class="profile-form">
          <label for="mfaToken">Authenticator code</label>
          <input id="mfaToken" v-model="mfaToken" inputmode="numeric" pattern="[0-9]{6}" maxlength="6" required />
          <button type="submit">Enable MFA</button>
        </form>
        <p v-if="mfaMessage" class="success" role="status">{{ mfaMessage }}</p>
        <p v-if="mfaError" class="error" role="alert">{{ mfaError }}</p>
      </section>

      <BillingOptions />
    </div>
  </PageWrapper>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import BillingOptions from './BillingOptions.vue'
import { changePassword as changePasswordApi, enableMfa, getAuthUser, setAuthUser, setupMfa } from '../../../services/authApi.js'
import { getProfile, updateProfile as updateProfileApi } from '../../../services/profileApi.js'

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
const loadingProfile = ref(false)
const profileError = ref('')
const savingPassword = ref(false)
const passwordChanged = ref(false)
const passwordError = ref('')
const mfaUri = ref('')
const mfaToken = ref('')
const mfaMessage = ref('')
const mfaError = ref('')
const settingUpMfa = ref(false)

async function startMfaSetup() {
  settingUpMfa.value = true
  mfaError.value = ''
  try {
    const result = await setupMfa()
    mfaUri.value = result.uri
    mfaMessage.value = 'Scan this URI in your authenticator app, then enter the six-digit code to enable MFA.'
  } catch (error) {
    mfaError.value = error.message
  } finally {
    settingUpMfa.value = false
  }
}

async function confirmMfaSetup() {
  mfaError.value = ''
  try {
    await enableMfa(mfaToken.value)
    mfaMessage.value = 'MFA enabled successfully.'
    mfaUri.value = ''
    mfaToken.value = ''
  } catch (error) {
    mfaError.value = error.message
  }
}

onMounted(async () => {
  const user = getAuthUser()
  if (!user) {
    router.push('/login')
    return
  }
  username.value = user.username || ''
  email.value = user.email || ''
  fullName.value = user.fullName || username.value

  loadingProfile.value = true
  profileError.value = ''
  try {
    const profile = await getProfile()
    username.value = profile.username || username.value
    email.value = profile.email || ''
    fullName.value = profile.fullName || ''
    bio.value = profile.bio || ''
    setAuthUser({ ...user, ...profile })
    window.dispatchEvent(new Event('authStateChanged'))
  } catch (error) {
    profileError.value = error.message
  } finally {
    loadingProfile.value = false
  }
})

async function updateProfile() {
  saving.value = true
  saved.value = false
  profileError.value = ''

  try {
    const result = await updateProfileApi({
      email: email.value,
      fullName: fullName.value?.trim() || username.value,
      bio: bio.value
    })
    const user = { ...getAuthUser(), ...result.user }
    username.value = user.username || username.value
    email.value = user.email || ''
    fullName.value = user.fullName || ''
    bio.value = user.bio || ''
    setAuthUser(user)
    window.dispatchEvent(new Event('authStateChanged'))
    saved.value = true
    setTimeout(() => {
      saved.value = false
    }, 3000)
  } catch (error) {
    profileError.value = error.message
  } finally {
    saving.value = false
  }
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
