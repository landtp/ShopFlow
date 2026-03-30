// src/pages/CreateOrderPage.tsx
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ordersApi } from '../api/orders'
import { useAuthStore } from '../store/authStore'
import toast from 'react-hot-toast'
import { Plus, Trash2 } from 'lucide-react'

interface OrderItemForm {
  productId:   string
  productName: string
  quantity:    number
  unitPrice:   number
  currency:    string
}

const DEFAULT_ITEM: OrderItemForm = {
  productId:   '',
  productName: '',
  quantity:    1,
  unitPrice:   0,
  currency:    'VND'
}

// Sample products để demo
const SAMPLE_PRODUCTS = [
  { id: '11111111-1111-1111-1111-111111111111', name: 'iPhone 15 Pro',    price: 29990000 },
  { id: '22222222-2222-2222-2222-222222222222', name: 'MacBook Pro M3',   price: 52990000 },
  { id: '33333333-3333-3333-3333-333333333333', name: 'iPad Pro M4',      price: 28990000 },
  { id: '44444444-4444-4444-4444-444444444444', name: 'AirPods Pro',      price: 6990000  },
  { id: '55555555-5555-5555-5555-555555555555', name: 'Apple Watch S9',   price: 11990000 },
]

export default function CreateOrderPage() {
  const navigate  = useNavigate()
  const userId    = useAuthStore(s => s.userId)
  const [address, setAddress] = useState('')
  const [items, setItems]     = useState<OrderItemForm[]>([{ ...DEFAULT_ITEM }])
  const [loading, setLoading] = useState(false)

  const addItem = () =>
    setItems(prev => [...prev, { ...DEFAULT_ITEM }])

  const removeItem = (idx: number) =>
    setItems(prev => prev.filter((_, i) => i !== idx))

  const updateItem = (idx: number, field: keyof OrderItemForm, value: string | number) =>
    setItems(prev => prev.map((item, i) =>
      i === idx ? { ...item, [field]: value } : item))

  const selectProduct = (idx: number, productId: string) => {
    const product = SAMPLE_PRODUCTS.find(p => p.id === productId)
    if (!product) return
    setItems(prev => prev.map((item, i) =>
      i === idx ? {
        ...item,
        productId:   product.id,
        productName: product.name,
        unitPrice:   product.price
      } : item))
  }

  const totalAmount = items.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity, 0)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!userId) {
      toast.error('Vui lòng đăng nhập lại')
      return
    }

    if (items.some(i => !i.productId)) {
      toast.error('Vui lòng chọn sản phẩm cho tất cả dòng')
      return
    }

    setLoading(true)
    try {
      const { data } = await ordersApi.createOrder({
        customerId:      userId,
        shippingAddress: address,
        items
      })

      toast.success('Đặt hàng thành công! Đang xử lý thanh toán...')
      navigate('/orders')
    } catch (err: any) {
      const errors = err.response?.data
      if (Array.isArray(errors))
        toast.error(errors[0])
      else
        toast.error(err.response?.data?.error || 'Đặt hàng thất bại')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-semibold text-gray-800 mb-6">
        Tạo đơn hàng mới
      </h1>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Shipping address */}
        <div className="bg-white border border-gray-100 rounded-2xl p-5">
          <h2 className="text-sm font-medium text-gray-700 mb-3">
            Địa chỉ giao hàng
          </h2>
          <input
            value={address}
            onChange={e => setAddress(e.target.value)}
            className="w-full border border-gray-200 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="123 Nguyen Hue, Q1, HCMC"
            required
          />
        </div>

        {/* Items */}
        <div className="bg-white border border-gray-100 rounded-2xl p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-medium text-gray-700">
              Sản phẩm
            </h2>
            <button
              type="button"
              onClick={addItem}
              className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-700"
            >
              <Plus size={12} /> Thêm sản phẩm
            </button>
          </div>

          <div className="space-y-3">
            {items.map((item, idx) => (
              <div key={idx}
                className="flex gap-3 items-start p-3 bg-gray-50 rounded-xl">
                {/* Product select */}
                <div className="flex-1">
                  <select
                    value={item.productId}
                    onChange={e => selectProduct(idx, e.target.value)}
                    className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                    required
                  >
                    <option value="">Chọn sản phẩm...</option>
                    {SAMPLE_PRODUCTS.map(p => (
                      <option key={p.id} value={p.id}>{p.name}</option>
                    ))}
                  </select>
                  {item.unitPrice > 0 && (
                    <p className="text-xs text-gray-400 mt-1 ml-1">
                      {item.unitPrice.toLocaleString('vi-VN')} VND / cái
                    </p>
                  )}
                </div>

                {/* Quantity */}
                <input
                  type="number"
                  min={1}
                  value={item.quantity}
                  onChange={e => updateItem(idx, 'quantity', Number(e.target.value))}
                  className="w-16 border border-gray-200 rounded-lg px-3 py-2 text-sm text-center focus:outline-none focus:ring-2 focus:ring-blue-500"
                />

                {/* Subtotal */}
                <div className="text-sm text-gray-600 pt-2 min-w-[100px] text-right">
                  {(item.unitPrice * item.quantity).toLocaleString('vi-VN')}
                </div>

                {/* Remove */}
                {items.length > 1 && (
                  <button
                    type="button"
                    onClick={() => removeItem(idx)}
                    className="pt-2 text-red-400 hover:text-red-600"
                  >
                    <Trash2 size={14} />
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Total */}
        <div className="bg-blue-50 rounded-2xl p-5 flex justify-between items-center">
          <span className="text-gray-600">Tổng cộng</span>
          <span className="text-xl font-semibold text-blue-700">
            {totalAmount.toLocaleString('vi-VN')} VND
          </span>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 text-white py-3 rounded-xl font-medium hover:bg-blue-700 disabled:opacity-50 transition"
        >
          {loading ? 'Đang đặt hàng...' : 'Đặt hàng ngay'}
        </button>
      </form>
    </div>
  )
}