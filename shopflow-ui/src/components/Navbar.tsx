// src/components/Navbar.tsx
import { Link, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { authApi } from '../api/auth'
import toast from 'react-hot-toast'
import { ShoppingBag, LogOut } from 'lucide-react'

export default function Navbar() {
  const navigate = useNavigate()
  const { firstName, isAuthenticated, logout } = useAuthStore()

  const handleLogout = async () => {
    try {
      await authApi.logout()
    } catch { }
    logout()
    toast.success('Đã đăng xuất')
    navigate('/login')
  }

  return (
    <nav className="bg-white border-b border-gray-100 px-4 py-3">
      <div className="max-w-4xl mx-auto flex items-center justify-between">
        <Link to="/orders"
          className="flex items-center gap-2 font-semibold text-gray-800">
          <ShoppingBag size={20} className="text-blue-600" />
          ShopFlow
        </Link>

        {isAuthenticated && (
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-500">
              Xin chào, <span className="text-gray-800 font-medium">{firstName}</span>
            </span>
            <button
              onClick={handleLogout}
              className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-red-500 transition"
            >
              <LogOut size={14} />
              Đăng xuất
            </button>
          </div>
        )}
      </div>
    </nav>
  )
}