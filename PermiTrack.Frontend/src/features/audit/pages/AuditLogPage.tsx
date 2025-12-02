import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Table, Card, Tag, Typography, Button, Modal, Descriptions, DatePicker, Space, Spin, Alert, Row, Col } from 'antd';
import { EyeOutlined } from '@ant-design/icons';
import auditLogService from '../services/auditService';
import type { AuditLog } from '../services/auditService';
import type { PaginatedResponse } from '../../../types/api.types';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

const formatDateShort = (iso?: string) => {
  if (!iso) return '';
  try {
    const d = new Date(iso);
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    const hh = String(d.getHours()).padStart(2, '0');
    const min = String(d.getMinutes()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd} ${hh}:${min}`;
  } catch {
    return iso;
  }
};

const AuditLogPage: React.FC = () => {
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  const [dateRange, setDateRange] = useState<any>(null);

  const dateFrom = dateRange?.[0]?.toISOString?.() ?? undefined;
  const dateTo = dateRange?.[1]?.toISOString?.() ?? undefined;

  const { data, isLoading, isError, error } = useQuery<PaginatedResponse<AuditLog>, Error>({
    queryKey: ['audit-logs', page, pageSize, dateFrom, dateTo],
    queryFn: () => auditLogService.getLogs({ page, pageSize, dateFrom, dateTo })
  });

  // DEBUG: Check data structure
  console.log('AuditLogPage Data:', { data, isLoading, isError, error });

  const tableData = Array.isArray(data?.data) ? data.data : [];
  const totalCount = data?.pagination?.totalCount ?? 0;

  const onTableChange = (pagination: any) => {
    setPage(pagination.current);
    setPageSize(pagination.pageSize);
  };

  const columns = [
    {
      title: 'Time',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (val: string) => formatDateShort(val),
      width: 180,
    },
    {
      title: 'User',
      dataIndex: 'userName',
      key: 'userName',
      render: (val: string) => <Text>{val ?? 'System'}</Text>,
      width: 160,
    },
    {
      title: 'Action',
      dataIndex: 'action',
      key: 'action',
      render: (val: string) => {
        const v = String(val).toLowerCase();
        let color = 'blue';
        if (v.includes('create')) color = 'green';
        if (v.includes('delete')) color = 'red';
        if (v.includes('update') || v.includes('edit')) color = 'cyan';
        return <Tag color={color}>{val}</Tag>;
      },
      width: 120,
    },
    {
      title: 'Resource',
      key: 'resource',
      render: (_: any, record: AuditLog) => (
        <Text>{`${record.resourceType ?? '-'}${record.resourceId ? ` / ${record.resourceId}` : ''}`}</Text>
      ),
    },
    {
      title: 'IP',
      dataIndex: 'ipAddress',
      key: 'ipAddress',
      width: 140,
    },
    {
      title: 'Action',
      key: 'actions',
      render: (_: any, record: AuditLog) => (
        <Button icon={<EyeOutlined />} onClick={() => setSelectedLog(record)}>
          View Details
        </Button>
      ),
      width: 140,
    },
  ];

  return (
    <div>
      <Row style={{ marginBottom: 16 }}>
        <Col span={24}>
          <Title level={3}>Audit Logs</Title>
          <Text type="secondary">Review system audit trail and changes</Text>
        </Col>
      </Row>

      <Card style={{ marginBottom: 16 }}>
        <Space direction="horizontal">
          <RangePicker onChange={(vals) => setDateRange(vals)} />
          <Button onClick={() => { setPage(1); /* trigger refetch by changing state */ }}>Filter</Button>
        </Space>
      </Card>

      <Card>
        {isLoading ? (
          <div style={{ display: 'flex', justifyContent: 'center', padding: 40 }}>
            <Spin />
          </div>
        ) : isError ? (
          <Alert title="Failed to load audit logs" description={error?.message} type="error" showIcon />
        ) : (
          <Table
            rowKey="id"
            columns={columns}
            dataSource={tableData}
            pagination={{
              current: page,
              pageSize,
              total: totalCount,
              showSizeChanger: true,
            }}
            onChange={(pag) => onTableChange(pag)}
          />
        )}
      </Card>

      <Modal
        title="Audit Log Details"
        open={!!selectedLog}
        onCancel={() => setSelectedLog(null)}
        footer={null}
        width={800}
      >
        {selectedLog && (
          <Descriptions column={1} bordered>
            <Descriptions.Item label="Time">{formatDateShort(selectedLog.createdAt)}</Descriptions.Item>
            <Descriptions.Item label="User">{selectedLog.userName ?? 'System'}</Descriptions.Item>
            <Descriptions.Item label="Action">{selectedLog.action}</Descriptions.Item>
            <Descriptions.Item label="Resource">{`${selectedLog.resourceType ?? ''} ${selectedLog.resourceId ?? ''}`}</Descriptions.Item>
            <Descriptions.Item label="IP Address">{selectedLog.ipAddress ?? '-'}</Descriptions.Item>
            <Descriptions.Item label="Old Values">
              <pre style={{ whiteSpace: 'pre-wrap' }}>{selectedLog.oldValues ?? ''}</pre>
            </Descriptions.Item>
            <Descriptions.Item label="New Values">
              <pre style={{ whiteSpace: 'pre-wrap' }}>{selectedLog.newValues ?? ''}</pre>
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>
    </div>
  );
};

export default AuditLogPage;
