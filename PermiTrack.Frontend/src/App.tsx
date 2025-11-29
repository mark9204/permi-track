import React from 'react';

import { Routes, Route } from 'react-router-dom';

import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';

import MainLayout from './components/layout/MainLayout';

import UserListPage from './features/users/pages/UserListPage';

import MyRequestsPage from './features/workflow/pages/MyRequestsPage';
import RequestApprovalPage from './features/workflow/pages/RequestApprovalPage';



const App: React.FC = () => {

  return (

    <Routes>

      {/* Publikus Route: Login */}

      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />



      {/* Védett Route-ok (Layout keretben) */}

      <Route path="/" element={<MainLayout />}>

        <Route index element={<div>Dashboard Statisztikák (Hamarosan)</div>} />

        <Route path="users" element={<UserListPage />} />

        <Route path="roles" element={<div>Role Kezelés (Hamarosan)</div>} />

        <Route path="my-access" element={<MyRequestsPage />} />
        <Route path="approvals" element={<RequestApprovalPage />} />

      </Route>

      {/* Fallback for unmatched routes to help debugging direct navigations */}
      <Route path="*" element={<div style={{ padding: 24 }}>Page not found (404). Check the URL or navigate from the app.</div>} />

    </Routes>

  );

};



export default App;