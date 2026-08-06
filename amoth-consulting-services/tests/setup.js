import { defineComponent, h } from 'vue'
import { vi } from 'vitest'

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn(), replace: vi.fn() }),
  useRoute: () => ({ params: { serviceId: 'strategy-transformation' }, path: '/' }),
  RouterLink: defineComponent({
    name: 'RouterLink',
    props: {
      to: {
        type: [String, Object],
        required: false,
      },
    },
    setup(_, { slots }) {
      return () => h('a', slots.default?.())
    },
  }),
}))
