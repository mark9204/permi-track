import React from 'react';
import { Alert, Button } from 'antd';

interface State {
  hasError: boolean;
  error?: Error | null;
}

class ErrorBoundary extends React.Component<React.PropsWithChildren<{}>, State> {
  state: State = { hasError: false, error: null };

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  componentDidCatch(_error: Error, _info: any) {
    // you could log the error to an external service here
    // console.error(error, info);
  }

  handleReload = () => {
    this.setState({ hasError: false, error: null });
    // reload the page to get a fresh app state
    window.location.reload();
  };

  render() {
    if (this.state.hasError) {
      return (
        <div style={{ padding: 24 }}>
          <Alert
            title="An unexpected error occurred"
            description={this.state.error?.message || 'Unknown error'}
            type="error"
            showIcon
          />
          <div style={{ marginTop: 16 }}>
            <Button onClick={this.handleReload}>Reload</Button>
          </div>
        </div>
      );
    }

    return this.props.children as React.ReactElement;
  }
}

export default ErrorBoundary;
