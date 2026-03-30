// src/App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { useAuthStore } from './store/authStore'
import Navbar from './components/Navbar'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import OrdersPage from './pages/OrdersPage'
import CreateOrderPage from './pages/CreateOrderPage'

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore(s => s.isAuthenticated)
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />
}

function PublicRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore(s => s.isAuthenticated)
  return isAuthenticated ? <Navigate to="/orders" replace /> : <>{children}</>
}

export default function App() {
  const isAuthenticated = useAuthStore(s => s.isAuthenticated)

  return (
    <BrowserRouter>
      <Toaster position="top-right" />
      {isAuthenticated && <Navbar />}

      <Routes>
        <Route path="/" element={<Navigate to="/orders" replace />} />

        <Route path="/login" element={
          <PublicRoute><LoginPage /></PublicRoute>
        } />

        <Route path="/register" element={
          <PublicRoute><RegisterPage /></PublicRoute>
        } />

        <Route path="/orders" element={
          <ProtectedRoute><OrdersPage /></ProtectedRoute>
        } />

        <Route path="/orders/create" element={
          <ProtectedRoute><CreateOrderPage /></ProtectedRoute>
        } />
      </Routes>
    </BrowserRouter>
  )
}