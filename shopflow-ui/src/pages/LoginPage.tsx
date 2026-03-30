// src/pages/LoginPage.tsx
import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { authApi } from '../api/auth'
import { useAuthStore } from '../store/authStore'
import toast from 'react-hot-toast'
import { ShoppingBag, Mail, Lock, ArrowRight } from 'lucide-react'

export default function LoginPage() {
  const navigate = useNavigate()
  const login    = useAuthStore(s => s.login)
  const [form, setForm] = useState({ email: '', password: '' })
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      const { data } = await authApi.login(form)
      login(data)
      toast.success(`Chào mừng ${data.firstName}!`)
      navigate('/orders')
    } catch (err: any) {
      toast.error(err.response?.data?.error || 'Email hoặc password không đúng')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex">

      {/* LEFT — Branding panel */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-blue-600 to-blue-800 flex-col justify-between p-12">
        <div className="flex items-center gap-3">
          <div className="bg-white/20 p-2 rounded-xl">
            <ShoppingBag size={24} className="text-white" />
          </div>
          <span className="text-white font-semibold text-xl">ShopFlow</span>
        </div>

        <div>
          <h1 className="text-4xl font-bold text-white leading-tight mb-4">
            E-Commerce<br />Microservices<br />Platform
          </h1>
          <p className="text-blue-200 text-sm leading-relaxed">
            Clean Architecture · DDD · CQRS<br />
            Kafka · Redis · JWT Auth
          </p>
        </div>

        <div className="flex gap-4">
          {['Order Service', 'Payment Service', 'Identity Service'].map(s => (
            <div key={s}
              className="bg-white/10 rounded-lg px-3 py-1.5 text-white text-xs">
              {s}
            </div>
          ))}
        </div>
      </div>

      {/* RIGHT — Login form */}
      <div className="flex-1 flex items-center justify-center bg-gray-50 p-8">
        <div className="w-full max-w-sm">

          {/* Mobile logo */}
          <div className="flex items-center gap-2 mb-8 lg:hidden">
            <div className="bg-blue-600 p-1.5 rounded-lg">
              <ShoppingBag size={18} className="text-white" />
            </div>
            <span className="font-semibold text-gray-800">ShopFlow</span>
          </div>

          <h2 className="text-2xl font-bold text-gray-800 mb-1">
            Đăng nhập
          </h2>
          <p className="text-sm text-gray-500 mb-8">
            Chào mừng trở lại! Nhập thông tin tài khoản của bạn.
          </p>

          <form onSubmit={handleSubmit} className="space-y-4">

            {/* Email */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Email
              </label>
              <div className="relative">
                <Mail size={15}
                  className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                <input
                  type="email"
                  value={form.email}
                  onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                  className="w-full pl-9 pr-4 py-2.5 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white transition"
                  placeholder="lan@shopflow.com"
                  required
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Password
              </label>
              <div className="relative">
                <Lock size={15}
                  className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                <input
                  type="password"
                  value={form.password}
                  onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
                  className="w-full pl-9 pr-4 py-2.5 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white transition"
                  placeholder="••••••••"
                  required
                />
              </div>
            </div>

            {/* Submit */}
            <button
              type="submit"
              disabled={loading}
              className="w-full flex items-center justify-center gap-2 bg-blue-600 text-white py-2.5 rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition mt-2"
            >
              {loading ? (
                <span className="flex items-center gap-2">
                  <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
                    <circle className="opacity-25" cx="12" cy="12" r="10"
                      stroke="currentColor" strokeWidth="4"/>
                    <path className="opacity-75" fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                  </svg>
                  Đang đăng nhập...
                </span>
              ) : (
                <>
                  Đăng nhập
                  <ArrowRight size={15} />
                </>
              )}
            </button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Chưa có tài khoản?{' '}
            <Link to="/register"
              className="text-blue-600 font-medium hover:underline">
              Đăng ký ngay
            </Link>
          </p>

          {/* Demo credentials */}
          <div className="mt-6 p-3 bg-blue-50 rounded-xl border border-blue-100">
            <p className="text-xs font-medium text-blue-700 mb-1">
              Demo account
            </p>
            <p className="text-xs text-blue-600">
              lan@shopflow.com / Password123
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
