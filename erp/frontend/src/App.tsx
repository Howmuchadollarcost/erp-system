import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { CssBaseline, ThemeProvider, createTheme } from '@mui/material';
import { AuthProvider, useAuth } from './auth/AuthContext';
import LoginPage from './auth/LoginPage';
import WorkerTimesheetPage from './pages/WorkerTimesheetPage';
import SupervisorReviewPage from './pages/SupervisorReviewPage';

const theme = createTheme();

function ProtectedRoute({ children, roles }: { children: React.ReactElement; roles?: string[] }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />;
  return children;
}

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <WorkerTimesheetPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/review"
              element={
                <ProtectedRoute roles={["Supervisor", "Admin"]}>
                  <SupervisorReviewPage />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}
