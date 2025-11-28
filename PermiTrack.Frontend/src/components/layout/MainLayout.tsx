import React from 'react';
import { Layout, Menu } from 'antd';
import { Outlet, Link } from 'react-router-dom';
import {
  DashboardOutlined,
  UserOutlined,
  SafetyCertificateOutlined
} from '@ant-design/icons';

const { Header, Content, Footer, Sider } = Layout;

const menuItems = [
  {
    key: 'dashboard',
    icon: <DashboardOutlined />,
    label: <Link to="/">Dashboard</Link>,
  },
  {
    key: 'users',
    icon: <UserOutlined />,
    label: <Link to="/users">Users</Link>,
  },
  {
    key: 'roles',
    icon: <SafetyCertificateOutlined />,
    label: <Link to="/roles">Roles</Link>,
  },
];

const MainLayout: React.FC = () => {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider breakpoint="lg" collapsedWidth="0">
        <div style={{ height: 32, margin: 16, background: 'rgba(255, 255, 255, 0.2)', borderRadius: 6 }} />
        <Menu theme="dark" mode="inline" defaultSelectedKeys={['dashboard']} items={menuItems} />
      </Sider>
      <Layout>
        <Header style={{ padding: 0, background: '#fff' }} />
        <Content style={{ margin: '24px 16px 0' }}>
          <div style={{ padding: 24, minHeight: 360, background: '#fff', borderRadius: 8 }}>
            <Outlet />
          </div>
        </Content>
        <Footer style={{ textAlign: 'center' }}>PermiTrack ©2025 Created by Architect Team</Footer>
      </Layout>
    </Layout>
  );
};

export default MainLayout;
