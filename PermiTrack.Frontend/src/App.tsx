import React from 'react';
import { Routes, Route } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import MainLayout from './components/layout/MainLayout';
import UserListPage from './features/users/pages/UserListPage';

const App: React.FC = () => {
  return (
    <Routes>
      {/* Publikus Route: Login */}
      <Route path="/login" element={<LoginPage />} />

      {/* Védett Route-ok (Layout keretben) */}
      <Route path="/" element={<MainLayout />}>
        <Route index element={<div>Dashboard Statisztikák (Hamarosan)</div>} />
        <Route path="users" element={<UserListPage />} />
        <Route path="roles" element={<div>Role Kezelés (Hamarosan)</div>} />
      </Route>
    </Routes>
  );
};

export default App;