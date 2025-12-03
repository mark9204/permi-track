import React from 'react';

import { Routes, Route, Navigate } from 'react-router-dom';

import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './features/dashboard/pages/DashboardPage';
import AuditLogPage from './features/audit/pages/AuditLogPage';

import MainLayout from './components/layout/MainLayout';

import UserListPage from './features/users/pages/UserListPage';
import RoleListPage from './features/roles/pages/RoleListPage';
import PermissionListPage from './features/permissions/pages/PermissionListPage';
import UserProfilePage from './features/users/pages/UserProfilePage';
import SystemConfigPage from './features/admin/pages/SystemConfigPage';

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

        
  <Route index element={<Navigate to="/dashboard" replace />} />
  <Route path="dashboard" element={<DashboardPage />} />
  <Route path="users" element={<UserListPage />} />
  <Route path="profile" element={<UserProfilePage />} />
  <Route path="audit-logs" element={<AuditLogPage />} />

        <Route path="roles" element={<RoleListPage />} />
        <Route path="permissions" element={<PermissionListPage />} />
        <Route path="system-config" element={<SystemConfigPage />} />

        <Route path="my-access" element={<MyRequestsPage />} />
        <Route path="approvals" element={<RequestApprovalPage />} />

      </Route>

      {/* Fallback for unmatched routes to help debugging direct navigations */}
      <Route path="*" element={<div style={{ padding: 24 }}>Page not found (404). Check the URL or navigate from the app.</div>} />

    </Routes>

  );

};



export default App;