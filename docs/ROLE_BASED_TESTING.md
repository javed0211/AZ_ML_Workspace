# Role-Based Testing Guide

## Overview

This guide covers testing different Azure ML workspace roles and permissions using the Python automation framework.

## Supported Roles

- **Owner**: Full access to workspace resources
- **Contributor**: Can create and manage resources
- **Reader**: Read-only access to workspace
- **Data Scientist**: ML-specific permissions
- **Data Labeler**: Data labeling permissions

## Configuration

Configure role-based testing in your `.env` file:
```env
# PIM Configuration
PIM_ROLE_NAME=your-pim-role
PIM_JUSTIFICATION=Automated testing

# Test User Credentials (if using multiple accounts)
TEST_USER_TENANT_ID=tenant-id
TEST_USER_CLIENT_ID=client-id
TEST_USER_CLIENT_SECRET=client-secret
```

## Running Role-Based Tests

```bash
# Run all role tests
python -m azure_ml_automation.cli test -p "roles"

# Run specific role tests
python -m azure_ml_automation.cli test -p "test_owner_permissions"
python -m azure_ml_automation.cli test -p "test_reader_permissions"

# Run with specific role context
AZURE_ROLE=Reader python -m azure_ml_automation.cli test -p "permissions"
```

## Test Structure

```
tests/roles/
├── test_owner_permissions.py       # Owner role tests
├── test_contributor_permissions.py # Contributor role tests
├── test_reader_permissions.py      # Reader role tests
├── test_data_scientist_permissions.py # Data Scientist role tests
└── test_pim_activation.py          # PIM role activation tests
```

## Writing Role-Based Tests

```python
from azure_ml_automation.tests.base_test import BaseTest
from azure_ml_automation.helpers.pim_manager import PIMManager

class TestOwnerPermissions(BaseTest):
    
    @pytest.mark.role("Owner")
    async def test_create_compute_instance(self):
        """Test compute instance creation with Owner role."""
        # Test owner-specific functionality
        pass
    
    @pytest.mark.role("Reader")
    async def test_read_only_access(self):
        """Test read-only access with Reader role."""
        # Test reader limitations
        pass
```

## PIM Integration

### Activating Roles

```python
from azure_ml_automation.helpers.pim_manager import PIMManager

async def test_with_pim_role():
    async with PIMManager() as pim:
        # Activate role
        await pim.activate_role("Data Scientist", "Testing ML models")
        
        # Run tests with activated role
        # ... test code ...
        
        # Role is automatically deactivated when context exits
```

### Role Validation

```python
async def test_role_permissions():
    # Verify current role
    current_role = await self.azure_helper.get_current_role()
    assert current_role in ["Owner", "Contributor"]
    
    # Test role-specific operations
    if current_role == "Owner":
        await self.test_owner_operations()
    elif current_role == "Contributor":
        await self.test_contributor_operations()
```

## Permission Testing Patterns

### 1. Positive Testing (Should Succeed)
```python
@pytest.mark.role("Owner")
async def test_owner_can_create_compute(self):
    """Owners should be able to create compute instances."""
    result = await self.azure_helper.create_compute_instance("test-compute")
    assert result.success
```

### 2. Negative Testing (Should Fail)
```python
@pytest.mark.role("Reader")
async def test_reader_cannot_create_compute(self):
    """Readers should not be able to create compute instances."""
    with pytest.raises(PermissionError):
        await self.azure_helper.create_compute_instance("test-compute")
```

### 3. UI Permission Testing
```python
@pytest.mark.role("Reader")
async def test_ui_elements_disabled_for_reader(self):
    """UI elements should be disabled for readers."""
    page = await self.get_workspace_page()
    
    # Check that create buttons are disabled
    create_button = page.locator("[data-testid='create-compute-button']")
    assert await create_button.is_disabled()
```

## Role-Specific Test Examples

### Owner Role Tests
```python
class TestOwnerPermissions(BaseTest):
    
    async def test_full_workspace_access(self):
        """Test full workspace management capabilities."""
        # Create resources
        # Modify settings
        # Delete resources
        pass
    
    async def test_user_management(self):
        """Test user and role management."""
        # Add users
        # Assign roles
        # Remove users
        pass
```

### Reader Role Tests
```python
class TestReaderPermissions(BaseTest):
    
    async def test_read_only_access(self):
        """Test read-only access to workspace resources."""
        # View resources
        # Cannot modify
        pass
    
    async def test_ui_restrictions(self):
        """Test UI restrictions for read-only users."""
        # Buttons disabled
        # Forms read-only
        pass
```

### Data Scientist Role Tests
```python
class TestDataScientistPermissions(BaseTest):
    
    async def test_ml_operations(self):
        """Test ML-specific operations."""
        # Create experiments
        # Train models
        # Register models
        pass
    
    async def test_compute_access(self):
        """Test compute instance access."""
        # Start/stop compute
        # Run notebooks
        pass
```

## Best Practices

1. **Role Isolation**: Test each role in isolation
2. **Permission Boundaries**: Test both allowed and forbidden operations
3. **UI Consistency**: Verify UI reflects permission levels
4. **Error Handling**: Test proper error messages for unauthorized operations
5. **Role Transitions**: Test switching between roles (PIM scenarios)

## Troubleshooting

### Authentication Issues
- Verify role assignments in Azure portal
- Check PIM activation status
- Validate service principal permissions

### Permission Errors
- Confirm role has required permissions
- Check resource-level permissions
- Verify subscription-level access

### PIM Issues
- Ensure PIM is configured correctly
- Check activation time limits
- Verify justification requirements