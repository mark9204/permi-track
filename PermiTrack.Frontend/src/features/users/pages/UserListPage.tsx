import { useState } from 'react';
import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { Table, Tag, Space, Button, Input, Avatar, Badge, Card, Row, Col } from 'antd';
import { EditOutlined, DeleteOutlined, SearchOutlined, UserOutlined, PlusOutlined } from '@ant-design/icons';
import { userService } from '../services/userService';
import UserDrawer from '../components/UserDrawer';
import { useUserPermissions } from '../../../hooks/useUserPermissions';
import type { User } from '../../../types/auth.types';
import type { ColumnsType } from 'antd/es/table';

const UserListPage = () => {
  const { isSuperAdmin, userDepartment } = useUserPermissions();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchText, setSearchText] = useState('');
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);

  const handleAdd = () => {
    setEditingUser(null);
    setIsDrawerOpen(true);
  };

  const handleEdit = (user: User) => {
    setEditingUser(user);
    setIsDrawerOpen(true);
  };

  const handleClose = () => {
    setIsDrawerOpen(false);
    setEditingUser(null);
  };

  const { data, isLoading } = useQuery({
    queryKey: ['users', page, pageSize, searchText],
    queryFn: () => userService.getUsers({ page, pageSize, search: searchText }),
    placeholderData: keepPreviousData,
    select: (data) => {
      if (isSuperAdmin) return data;
      // Filter by department for non-super admins
      const filteredData = data.data.filter(u => u.department === userDepartment);
      return { ...data, data: filteredData, total: filteredData.length };
    }
  });

  const columns: ColumnsType<User> = [
    {
      title: 'User',
      key: 'user',
      render: (_, record) => (
        <Space>
          <Avatar icon={<UserOutlined />} src={undefined}>
            {record.firstName?.[0]?.toUpperCase()}
          </Avatar>
          <div style={{ display: 'flex', flexDirection: 'column' }}>
            <span style={{ fontWeight: 500 }}>{`${record.firstName} ${record.lastName}`}</span>
            <span style={{ fontSize: '12px', color: '#888' }}>{record.username}</span>
          </div>
        </Space>
      ),
    },
    {
      title: 'Department',
      dataIndex: 'department',
      key: 'department',
      render: (dept: string) => dept ? <Tag>{dept}</Tag> : <Tag>N/A</Tag>,
    },
    {
      title: 'Roles',
      dataIndex: 'roles',
      key: 'roles',
      render: (roles: string[]) => (
        <>
          {roles?.map((role) => {
            let color = 'geekblue';
            if (role === 'Admin') color = 'volcano';
            if (role === 'Manager') color = 'green';
            return (
              <Tag color={color} key={role}>
                {role.toUpperCase()}
              </Tag>
            );
          })}
        </>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive: boolean) => (
        <Badge status={isActive ? 'success' : 'error'} text={isActive ? 'Active' : 'Inactive'} />
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="middle">
          <Button 
            icon={<EditOutlined />} 
            onClick={() => handleEdit(record)} 
          />
          <Button 
            danger 
            icon={<DeleteOutlined />} 
            onClick={() => console.log('Delete', record.id)} 
          />
        </Space>
      ),
    },
  ];

  return (
    <Card>
      <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
        <Col>
          <h1 style={{ margin: 0, fontSize: '24px' }}>Users</h1>
        </Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            Add User
          </Button>
        </Col>
      </Row>

      <div style={{ marginBottom: 16 }}>
        <Input
          placeholder="Search users..."
          prefix={<SearchOutlined />}
          onChange={(e) => setSearchText(e.target.value)}
          style={{ width: 300 }}
          allowClear
        />
      </div>

      <Table
        columns={columns}
        dataSource={data?.data}
        rowKey="id"
        loading={isLoading}
        pagination={{
          current: page,
          pageSize: pageSize,
          total: data?.pagination.totalCount,
          onChange: (p, ps) => {
            setPage(p);
            setPageSize(ps);
          },
          showSizeChanger: true,
        }}
      />
      <UserDrawer
        open={isDrawerOpen}
        onClose={handleClose}
        userToEdit={editingUser}
      />
    </Card>
  );
};

export default UserListPage;
