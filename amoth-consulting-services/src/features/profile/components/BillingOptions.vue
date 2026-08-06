<template>
  <section class="billing-section">
    <h2>Billing & Payments</h2>
    <br />

    <div class="invoices-container">
      <h3>Outstanding Invoices</h3>
      <div v-if="invoices.length === 0" class="no-invoices">
        <p>No outstanding invoices at this time.</p>
      </div>

      <div v-else class="invoices-list">
        <div v-for="invoice in invoices" :key="invoice.id" class="invoice-card">
          <div class="invoice-header">
            <span class="invoice-id">Invoice #{{ invoice.id }}</span>
            <span :class="['status', invoice.status.toLowerCase()]">{{ invoice.status }}</span>
          </div>
          <div class="invoice-details">
            <div class="detail-row">
              <span class="label">Amount Due:</span>
              <span class="amount">${{ invoice.amount.toFixed(2) }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Due Date:</span>
              <span>{{ formatDate(invoice.dueDate) }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Description:</span>
              <span>{{ invoice.description }}</span>
            </div>
          </div>
          <button
            type="button"
            v-if="invoice.status === 'Pending'"
            @click="selectInvoice(invoice)"
            class="btn-pay"
          >
            Pay Now
          </button>
        </div>
      </div>
    </div>

    <div v-if="selectedInvoice" class="payment-form-container">
      <h3>Payment Details</h3>
      <form @submit.prevent="processPayment" class="payment-form">
        <div class="form-section">
          <h4>Invoice Information</h4>
          <div class="field">
            <label><span class="label-text">Invoice #</span></label>
            <input type="text" :value="selectedInvoice.id" disabled />
          </div>
          <div class="field">
            <label><span class="label-text">Amount to Pay</span></label>
            <input type="text" :value="'$' + selectedInvoice.amount.toFixed(2)" disabled />
          </div>
        </div>

        <div class="form-section">
          <h4>Payment Method</h4>
          <div class="payment-methods">
            <div class="method-option">
              <input id="credit-card" v-model="paymentMethod" type="radio" value="credit_card" />
              <label for="credit-card">Credit Card</label>
            </div>
            <div class="method-option">
              <input id="bank-transfer" v-model="paymentMethod" type="radio" value="bank_transfer" />
              <label for="bank-transfer">Bank Transfer</label>
            </div>
            <div class="method-option">
              <input id="ach" v-model="paymentMethod" type="radio" value="ach" />
              <label for="ach">ACH Payment</label>
            </div>
          </div>
        </div>

        <div v-if="paymentMethod === 'credit_card'" class="form-section">
          <h4>Credit Card Information</h4>
          <div class="field">
            <label for="cardName"><span class="label-text">Cardholder Name <span class="required">*</span></span></label>
            <input id="cardName" v-model="cardholderName" type="text" placeholder="Full Name on Card" required />
          </div>
          <div class="field">
            <label for="cardNumber"><span class="label-text">Card Number <span class="required">*</span></span></label>
            <input id="cardNumber" v-model="cardNumber" type="text" placeholder="1234 5678 9012 3456" maxlength="19" required />
          </div>
          <div class="field-row">
            <div class="field">
              <label for="expiry"><span class="label-text">Expiry Date <span class="required">*</span></span></label>
              <input id="expiry" v-model="cardExpiry" type="text" placeholder="MM/YY" maxlength="5" required />
            </div>
            <div class="field">
              <label for="cvv"><span class="label-text">CVV <span class="required">*</span></span></label>
              <input id="cvv" v-model="cardCVV" type="text" placeholder="123" maxlength="3" required />
            </div>
          </div>
        </div>

        <div v-if="paymentMethod === 'bank_transfer'" class="form-section">
          <h4>Bank Transfer Information</h4>
          <div class="info-box">
            <p><strong>Bank Name:</strong> First National Bank</p>
            <p><strong>Account Number:</strong> ****5678</p>
            <p><strong>Routing Number:</strong> 021000021</p>
            <p><strong>Reference:</strong> Invoice #{{ selectedInvoice.id }}</p>
          </div>
          <div class="field">
            <label for="bankReference"><span class="label-text">Confirmation Code (if available)</span></label>
            <input id="bankReference" v-model="bankReference" type="text" placeholder="Your bank's confirmation code" />
          </div>
        </div>

        <div v-if="paymentMethod === 'ach'" class="form-section">
          <h4>ACH Account Information</h4>
          <div class="field">
            <label for="achRoutingNumber"><span class="label-text">Routing Number <span class="required">*</span></span></label>
            <input id="achRoutingNumber" v-model="achRoutingNumber" type="text" placeholder="9 digit routing number" required />
          </div>
          <div class="field">
            <label for="achAccountNumber"><span class="label-text">Account Number <span class="required">*</span></span></label>
            <input id="achAccountNumber" v-model="achAccountNumber" type="text" placeholder="Your account number" required />
          </div>
          <div class="field">
            <label for="achAccountType"><span class="label-text">Account Type <span class="required">*</span></span></label>
            <select id="achAccountType" v-model="achAccountType" required>
              <option value="">Select Account Type</option>
              <option value="checking">Checking</option>
              <option value="savings">Savings</option>
            </select>
          </div>
        </div>

        <div class="form-actions">
          <button type="submit" :disabled="processing" class="btn-submit">
            {{ processing ? 'Processing…' : 'Pay Invoice' }}
          </button>
          <button type="button" @click="cancelPayment" class="btn-cancel">
            Cancel
          </button>
        </div>

        <p v-if="paymentError" class="error" role="alert">{{ paymentError }}</p>
        <p v-if="paymentSuccess" class="success" role="alert">{{ paymentSuccess }}</p>
      </form>
    </div>
  </section>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const invoices = ref([])
const selectedInvoice = ref(null)

const paymentMethod = ref('credit_card')
const cardholderName = ref('')
const cardNumber = ref('')
const cardExpiry = ref('')
const cardCVV = ref('')
const bankReference = ref('')
const achRoutingNumber = ref('')
const achAccountNumber = ref('')
const achAccountType = ref('')

const processing = ref(false)
const paymentError = ref('')
const paymentSuccess = ref('')

onMounted(() => {
  loadInvoices()
})

function loadInvoices() {
  invoices.value = [
    {
      id: 'INV-2024-001',
      amount: 1500.00,
      dueDate: '2024-07-15',
      description: 'Consulting Services - Q2 2024',
      status: 'Pending'
    },
    {
      id: 'INV-2024-002',
      amount: 2250.00,
      dueDate: '2024-08-01',
      description: 'Development Work - Project X',
      status: 'Pending'
    },
    {
      id: 'INV-2024-003',
      amount: 800.00,
      dueDate: '2024-06-30',
      description: 'Support Services - Monthly',
      status: 'Overdue'
    }
  ]

  const savedInvoices = localStorage.getItem('userInvoices')
  if (savedInvoices) {
    try {
      invoices.value = JSON.parse(savedInvoices)
    } catch (e) {
      console.error('Error loading invoices from localStorage', e)
    }
  }
}

function formatDate(dateString) {
  const options = { year: 'numeric', month: 'long', day: 'numeric' }
  return new Date(dateString).toLocaleDateString('en-US', options)
}

function selectInvoice(invoice) {
  if (!invoice || invoice.status !== 'Pending') {
    return
  }

  selectedInvoice.value = invoice
  resetPaymentForm()
  paymentError.value = ''
  paymentSuccess.value = ''
}

function resetPaymentForm() {
  cardholderName.value = ''
  cardNumber.value = ''
  cardExpiry.value = ''
  cardCVV.value = ''
  bankReference.value = ''
  achRoutingNumber.value = ''
  achAccountNumber.value = ''
  achAccountType.value = ''
}

function cancelPayment() {
  selectedInvoice.value = null
  resetPaymentForm()
  paymentError.value = ''
  paymentSuccess.value = ''
}

function processPayment() {
  paymentError.value = ''
  paymentSuccess.value = ''

  if (paymentMethod.value === 'credit_card') {
    if (!cardholderName.value || !cardNumber.value || !cardExpiry.value || !cardCVV.value) {
      paymentError.value = 'Please fill in all required credit card fields'
      return
    }
    if (cardNumber.value.replace(/\s/g, '').length < 13) {
      paymentError.value = 'Invalid card number'
      return
    }
  }

  if (paymentMethod.value === 'ach') {
    if (!achRoutingNumber.value || !achAccountNumber.value || !achAccountType.value) {
      paymentError.value = 'Please fill in all required ACH fields'
      return
    }
    if (achRoutingNumber.value.length !== 9) {
      paymentError.value = 'Routing number must be 9 digits'
      return
    }
  }

  processing.value = true

  setTimeout(() => {
    processing.value = false

    const invoiceIndex = invoices.value.findIndex(inv => inv.id === selectedInvoice.value.id)
    if (invoiceIndex !== -1) {
      invoices.value[invoiceIndex].status = 'Paid'
      localStorage.setItem('userInvoices', JSON.stringify(invoices.value))
    }

    paymentSuccess.value = `Payment of $${selectedInvoice.value.amount.toFixed(2)} for Invoice #${selectedInvoice.value.id} processed successfully!`

    setTimeout(() => {
      cancelPayment()
    }, 2000)
  }, 1500)
}
</script>

<style scoped>
.billing-section {
  margin-bottom: 3rem;
  padding: 1.5rem;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-background-soft);
}

.billing-section h2 {
  color: var(--color-heading);
  margin-bottom: 1rem;
}

.billing-section h3 {
  color: var(--color-heading);
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.billing-section h4 {
  color: var(--color-text);
  margin-bottom: 1rem;
  font-size: 1rem;
  font-weight: 600;
}

.invoices-container {
  margin-bottom: 2rem;
}

.no-invoices {
  padding: 2rem;
  text-align: center;
  background: var(--color-background);
  border-radius: 4px;
  color: var(--color-text-secondary);
}

.invoices-list {
  display: grid;
  gap: 1rem;
}

.invoice-card {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 1rem;
  background: var(--color-background);
}

.invoice-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.invoice-id {
  font-weight: 600;
  color: var(--color-heading);
}

.status {
  padding: 0.25rem 0.5rem;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
}

.status.pending { background: #fff7e6; color: #b57b00; }
.status.overdue { background: #ffe8e8; color: #b02a1d; }
.status.paid { background: #e6f8ed; color: #0c7a3f; }

.invoice-details {
  display: grid;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
}

.label {
  font-weight: 600;
  color: var(--color-text);
}

.amount {
  font-weight: 700;
  color: var(--accent);
}

.btn-pay,
.btn-submit,
.btn-cancel {
  padding: 0.6rem 1rem;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-weight: 600;
}

.btn-pay,
.btn-submit {
  background: var(--accent);
  color: white;
}

.btn-cancel {
  background: #e2e8f0;
  color: #1e293b;
}

.payment-form-container {
  border-top: 1px solid var(--color-border);
  padding-top: 1.5rem;
}

.payment-form {
  display: grid;
  gap: 1rem;
}

.form-section {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 1rem;
  background: var(--color-background);
}

.field,
.field-row {
  display: grid;
  gap: 0.4rem;
  margin-bottom: 0.8rem;
}

.field-row {
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
}

input,
select {
  padding: 0.7rem;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-background);
  color: var(--color-text);
}

.payment-methods {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.method-option {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.info-box {
  padding: 0.9rem;
  border-radius: 6px;
  background: var(--color-background-soft);
  color: var(--color-text);
}

.form-actions {
  display: flex;
  gap: 0.75rem;
}

.error {
  color: #b02a1d;
}

.success {
  color: #0c7a3f;
}

.required {
  color: #b02a1d;
}
</style>
