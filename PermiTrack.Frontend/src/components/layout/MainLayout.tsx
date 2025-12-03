import React, { useEffect } from 'react';
import { Layout, Menu, Avatar, Dropdown, theme } from 'antd';
import type { MenuProps } from 'antd';
import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import {
  DashboardOutlined,
  UserOutlined,
  SafetyCertificateOutlined,
  KeyOutlined,
  AuditOutlined,
  SecurityScanOutlined,
  LogoutOutlined,
  LockOutlined,
  AppstoreOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';

const { Header, Content, Footer, Sider } = Layout;

// Ha nincs ErrorBoundary komponensed, ezt a sort és a lenti taget vedd ki!
// import ErrorBoundary from '../ErrorBoundary'; 

const MainLayout: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuthStore();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
    }
  }, [isAuthenticated, navigate]);

  
  const {
    token: { colorBgContainer },
  } = theme.useToken();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const userMenu: MenuProps['items'] = [
    {
      key: 'profile',
      label: <Link to="/profile">Profile</Link>,
      icon: <UserOutlined />,
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      label: 'Logout',
      icon: <LogoutOutlined />,
      onClick: handleLogout,
    },
  ];

  // Determine admin role robustly based on Level >= 2
  const rawRoles = user?.roles ?? [];
  
  const isAdmin = Array.isArray(rawRoles) && rawRoles.some((r: any) => {
    if (!r) return false;
    
    // Check if role is an object
    if (typeof r === 'object') {
      // 1. Check Level (handles number or string "2")
      const level = r.Level ?? r.level;
      if (level !== undefined && level !== null) {
        const parsed = Number(level);
        if (!isNaN(parsed) && parsed >= 2) return true;
      }

      // 2. Fallback: Check name for 'admin' inside object (in case Level is missing)
      const name = r.name || r.roleName || r.role || '';
      const lowerName = String(name).toLowerCase();
      return lowerName === 'admin' || lowerName === 'superadmin';
    }

    // Fallback for string roles
    if (typeof r === 'string') {
      const lower = r.toLowerCase();
      return lower === 'admin' || lower === 'superadmin';
    }
    return false;
  });

  const isSuperAdmin = Array.isArray(rawRoles) && rawRoles.some((r: any) => {
    if (!r) return false;
    if (typeof r === 'object') {
      const level = r.Level ?? r.level;
      if (level !== undefined && level !== null) {
        const parsed = Number(level);
        if (!isNaN(parsed) && parsed >= 3) return true;
      }
      const name = r.name || r.roleName || r.role || '';
      return String(name).toLowerCase() === 'superadmin';
    }
    
    if (typeof r === 'string') {
      return r.toLowerCase() === 'superadmin';
    }
    return false;
  });

  const menuItemsBase: MenuProps['items'] = [
    {
      key: '/dashboard',
      icon: <DashboardOutlined />,
      label: <Link to="/dashboard">Dashboard</Link>,
    },
    {
      key: '/my-access',
      icon: <KeyOutlined />,
      label: <Link to="/my-access">My requests</Link>,
    },
  ];

  if (isAdmin) {
    menuItemsBase.push({
      key: '/approvals',
      icon: <AuditOutlined />,
      label: <Link to="/approvals">Approvals</Link>,
    });

    menuItemsBase.push({
      key: '/users',
      icon: <UserOutlined />,
      label: <Link to="/users">Users</Link>,
    });
    
    menuItemsBase.push({
      key: '/roles',
      icon: <SafetyCertificateOutlined />,
      label: <Link to="/roles">Roles</Link>,
    });

    menuItemsBase.push({
      key: '/permissions',
      icon: <LockOutlined />,
      label: <Link to="/permissions">Permissions</Link>,
    });

    menuItemsBase.push({
      key: '/certifications',
      icon: <SafetyCertificateOutlined />,
      label: <Link to="/certifications">Access Reviews</Link>,
    });
  }
    
  if (isSuperAdmin) {
    menuItemsBase.push({
      key: 'admin',
      icon: <SettingOutlined />,
      label: 'Administration',
      children: [
        {
          key: '/system-config',
          icon: <AppstoreOutlined />,
          label: <Link to="/system-config">System Config</Link>,
        },
        {
          key: '/audit-logs',
          icon: <SecurityScanOutlined />,
          label: <Link to="/audit-logs">Audit Logs</Link>,
        },
      ]
    });
  }

  const menuItems: MenuProps['items'] = menuItemsBase;

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider breakpoint="lg" collapsedWidth="0">
        <div style={{ 
          height: 32, 
          margin: 16, 
          background: 'rgba(255, 255, 255, 0.2)', 
          borderRadius: 6,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'white',
          fontWeight: 'bold',
          letterSpacing: '1px'
        }}>
          PermiTrack
        </div>
        <Menu 
          theme="dark" 
          mode="inline" 
          selectedKeys={[location.pathname]} 
          items={menuItems} 
        />
      </Sider>
      <Layout>
        <Header style={{ padding: '0 24px', background: colorBgContainer, display: 'flex', justifyContent: 'flex-end', alignItems: 'center' }}>
          <Dropdown menu={{ items: userMenu }} placement="bottomRight">
            <div style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{ fontWeight: 500 }}>
                {user?.firstName} {user?.lastName}
              </span>
              <Avatar style={{ backgroundColor: '#1890ff' }} icon={<UserOutlined />} />
            </div>
          </Dropdown>
        </Header>
        
        <Content style={{ margin: '24px 16px 0' }}>
          <div style={{ padding: 24, minHeight: 360 }}>
            {/* Ha nincs ErrorBoundary-d, csak simán <Outlet /> legyen itt */}
            <Outlet />
          </div>
        </Content>
        <Footer style={{ textAlign: 'center' }}>PermiTrack ©2025</Footer>
      </Layout>
    </Layout>
  );
};

export default MainLayout;