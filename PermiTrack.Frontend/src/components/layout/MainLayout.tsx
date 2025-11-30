import React from 'react';
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
  LogoutOutlined
} from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';

const { Header, Content, Footer, Sider } = Layout;

// Ha nincs ErrorBoundary komponensed, ezt a sort és a lenti taget vedd ki!
// import ErrorBoundary from '../ErrorBoundary'; 

const MainLayout: React.FC = () => {
  const { user, logout } = useAuthStore();
  const location = useLocation();
  const navigate = useNavigate();
  
  const {
    token: { colorBgContainer },
  } = theme.useToken();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const userMenu: MenuProps['items'] = [
    {
      key: 'logout',
      label: 'Logout',
      icon: <LogoutOutlined />,
      onClick: handleLogout,
    },
  ];

  // Determine admin role robustly
  const rawRoles = user?.roles ?? [];
  const normalizedRoles: string[] = Array.isArray(rawRoles)
    ? rawRoles
        .map((r: any) => {
          if (!r) return '';
          if (typeof r === 'string') return r;
          return r.name || r.roleName || r.role || r.title || '';
        })
        .filter(Boolean)
    : [];

  const isAdmin = normalizedRoles.some((x) => String(x).toLowerCase() === 'admin');

  // DEBUG: Check why admin menu isn't showing
  console.log('MainLayout Debug:', { user, rawRoles, normalizedRoles, isAdmin });

  const menuItemsBase: MenuProps['items'] = [
    {
      key: '/dashboard',
      icon: <DashboardOutlined />,
      label: <Link to="/dashboard">Dashboard</Link>,
    },
    {
      key: '/my-access',
      icon: <KeyOutlined />,
      label: <Link to="/my-access">My Access</Link>,
    },
  ];

  if (isAdmin) {
    menuItemsBase.push({
      key: '/approvals',
      icon: <AuditOutlined />,
      label: <Link to="/approvals">Approvals</Link>,
    });
    
    menuItemsBase.push({
      key: '/audit-logs',
      icon: <SecurityScanOutlined />,
      label: <Link to="/audit-logs">Audit Logs</Link>,
    });

    menuItemsBase.push({ type: 'divider' });
    
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