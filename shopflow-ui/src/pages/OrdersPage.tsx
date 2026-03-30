// src/pages/OrdersPage.tsx
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ordersApi } from '../api/orders'
import { useAuthStore } from '../store/authStore'
import type  { Order } from '../types'
import toast from 'react-hot-toast'
import { Plus, RefreshCw } from 'lucide-react'

const STATUS_COLORS: Record<string, string> = {
  Pending:   'bg-yellow-100 text-yellow-700',
  Confirmed: 'bg-green-100 text-green-700',
  Cancelled: 'bg-red-100 text-red-700',
  Shipped:   'bg-blue-100 text-blue-700',
  Delivered: 'bg-purple-100 text-purple-700',
}

export default function OrdersPage() {
  const userId = useAuthStore(s => s.userId)
  const [orders, setOrders]   = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [page, setPage]       = useState(1)
  const [totalPages, setTotalPages] = useState(1)

  const fetchOrders = async () => {
    setLoading(true)
    try {
      const { data } = await ordersApi.getOrders(
        userId ?? undefined, page)
      setOrders(data.items)
      setTotalPages(data.totalPages)
    } catch {
      toast.error('Không thể tải danh sách đơn hàng')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchOrders() }, [page])

  // Auto refresh mỗi 5 giây để xem payment status
  useEffect(() => {
    const interval = setInterval(fetchOrders, 5000)
    return () => clearInterval(interval)
  }, [page])

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-800">
          Đơn hàng của tôi
        </h1>
        <div className="flex gap-3">
          <button
            onClick={fetchOrders}
            className="flex items-center gap-2 px-4 py-2 border border-gray-200 rounded-lg text-sm hover:bg-gray-50 transition"
          >
            <RefreshCw size={14} />
            Refresh
          </button>
          <Link
            to="/orders/create"
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 transition"
          >
            <Plus size={14} />
            Tạo đơn mới
          </Link>
        </div>
      </div>

      {loading && orders.length === 0 ? (
        <div className="text-center py-16 text-gray-400">Đang tải...</div>
      ) : orders.length === 0 ? (
        <div className="text-center py-16 text-gray-400">
          Chưa có đơn hàng nào.{' '}
          <Link to="/orders/create" className="text-blue-600 hover:underline">
            Tạo ngay
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map(order => (
            <div
              key={order.id}
              className="bg-white border border-gray-100 rounded-2xl p-5 shadow-sm"
            >
              <div className="flex items-start justify-between mb-3">
                <div>
                  <p className="text-xs text-gray-400 mb-1">
                    #{order.id.slice(0, 8).toUpperCase()}
                  </p>
                  <p className="text-sm text-gray-600">
                    {order.shippingAddress}
                  </p>
                </div>
                <span className={`text-xs font-medium px-3 py-1 rounded-full ${STATUS_COLORS[order.status] ?? 'bg-gray-100 text-gray-600'}`}>
                  {order.status}
                </span>
              </div>

              <div className="divide-y divide-gray-50">
                {order.items.map(item => (
                  <div key={item.id}
                    className="flex justify-between py-2 text-sm">
                    <span className="text-gray-700">
                      {item.productName} × {item.quantity}
                    </span>
                    <span className="text-gray-500">
                      {item.subTotal.toLocaleString('vi-VN')} VND
                    </span>
                  </div>
                ))}
              </div>

              <div className="flex items-center justify-between mt-3 pt-3 border-t border-gray-50">
                <span className="text-sm text-gray-400">
                  {new Date(order.createdAt).toLocaleString('vi-VN')}
                </span>
                <span className="font-semibold text-gray-800">
                  {order.totalAmount.toLocaleString('vi-VN')} {order.currency}
                </span>
              </div>

              {/* Payment status indicator */}
              {order.status === 'Pending' && (
                <div className="mt-3 flex items-center gap-2 text-xs text-yellow-600 bg-yellow-50 rounded-lg px-3 py-2">
                  <span className="animate-pulse">●</span>
                  Đang xử lý thanh toán...
                </div>
              )}
              {order.status === 'Confirmed' && (
                <div className="mt-3 flex items-center gap-2 text-xs text-green-600 bg-green-50 rounded-lg px-3 py-2">
                  ✓ Thanh toán thành công
                </div>
              )}
              {order.status === 'Cancelled' && (
                <div className="mt-3 flex items-center gap-2 text-xs text-red-600 bg-red-50 rounded-lg px-3 py-2">
                  ✗ Thanh toán thất bại — đơn hàng đã huỷ
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-8">
          <button
            disabled={page === 1}
            onClick={() => setPage(p => p - 1)}
            className="px-4 py-2 border rounded-lg text-sm disabled:opacity-40"
          >
            Trước
          </button>
          <span className="px-4 py-2 text-sm text-gray-500">
            {page} / {totalPages}
          </span>
          <button
            disabled={page === totalPages}
            onClick={() => setPage(p => p + 1)}
            className="px-4 py-2 border rounded-lg text-sm disabled:opacity-40"
          >
            Sau
          </button>
        </div>
      )}
    </div>
  )
}