import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, fireEvent, screen, waitFor } from '@testing-library/vue'
import BillingOptions from '../features/profile/components/BillingOptions.vue'

vi.stubGlobal('localStorage', {
  data: {},
  getItem(key) {
    return this.data[key] ?? null
  },
  setItem(key, value) {
    this.data[key] = String(value)
  },
  removeItem(key) {
    delete this.data[key]
  },
  clear() {
    this.data = {}
  },
})

describe('BillingOptions', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('allows paying an invoice and updates its status to Paid', async () => {
    render(BillingOptions)

    const payButtons = await screen.findAllByRole('button', { name: /pay now/i })
    expect(payButtons.length).toBeGreaterThan(0)

    await fireEvent.click(payButtons[0])

    const cardNameInput = await screen.findByLabelText(/Cardholder Name/i)
    const cardNumberInput = await screen.findByLabelText(/Card Number/i)
    const expiryInput = await screen.findByLabelText(/Expiry Date/i)
    const cvvInput = await screen.findByLabelText(/CVV/i)

    await fireEvent.update(cardNameInput, 'Test User')
    await fireEvent.update(cardNumberInput, '4111 1111 1111 1111')
    await fireEvent.update(expiryInput, '12/34')
    await fireEvent.update(cvvInput, '123')

    const payInvoiceButton = await screen.findByRole('button', { name: /pay invoice/i })
    await fireEvent.click(payInvoiceButton)

    await waitFor(() => {
      expect(screen.getByText(/processed successfully/i)).toBeTruthy()
    }, { timeout: 3000 })

    await waitFor(() => {
      expect(screen.getByText(/paid/i)).toBeTruthy()
    }, { timeout: 3000 })
  })

  it('opens the payment form when an invoice is selected and hides it when canceled', async () => {
    render(BillingOptions)

    const payButtons = await screen.findAllByRole('button', { name: /pay now/i })
    expect(payButtons.length).toBeGreaterThan(0)

    await fireEvent.click(payButtons[0])

    expect(await screen.findByText(/payment details/i)).toBeTruthy()

    const cancelButton = await screen.findByRole('button', { name: /cancel/i })
    await fireEvent.click(cancelButton)

    await waitFor(() => {
      expect(screen.queryByText(/payment details/i)).toBeNull()
    })
  })
})
