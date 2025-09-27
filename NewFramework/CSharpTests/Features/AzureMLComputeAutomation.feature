Feature: Azure ML Compute Instance Automation
    As a data scientist or ML engineer
    I want to automate the process of launching Linux-based compute instances from an ML workspace
    So that I can quickly establish VS Code remote connections and synchronize files between local and remote environments

Background:
    Given I have valid Azure ML workspace credentials
    And I have the necessary prerequisites installed
    And I have network connectivity to Azure services

@Prerequisites
Scenario: Validate Prerequisites for Automation
    When I validate all prerequisites for Azure ML automation
    Then Python should be installed and accessible
    And VS Code should be installed and accessible
    And Azure CLI should be installed and accessible
    And network connectivity to Azure ML should be available
    And SSH configuration should be available or generatable

@Authentication
Scenario: Authenticate with Azure ML Workspace
    Given I have valid Azure credentials
    When I initialize the Azure client
    Then the authentication should succeed
    And I should be able to access the ML workspace
    And the workspace should be available and accessible

@ComputeInstance
Scenario: Create and Manage Compute Instance
    Given I am authenticated with Azure ML
    When I create a new compute instance with name "test-automation-instance"
    And I specify VM size "Standard_DS3_v2"
    Then the compute instance should be created successfully
    And the instance should be in "Running" state
    And I should be able to retrieve instance details
    
@ComputeInstance
Scenario: Start and Stop Compute Instance
    Given I have a compute instance "test-automation-instance"
    When I stop the compute instance
    Then the instance should be stopped successfully
    When I start the compute instance
    Then the instance should be started successfully
    And the instance should be accessible

@SSH
Scenario: Setup SSH Connection to Compute Instance
    Given I have a running compute instance "test-automation-instance"
    When I generate SSH key pair if not exists
    Then the SSH keys should be created successfully
    When I setup SSH connection configuration
    Then the SSH config should be updated with instance details
    And I should be able to test SSH connectivity

@VSCode
Scenario: Setup VS Code Remote Connection
    Given I have SSH connection configured for "test-automation-instance"
    When I setup VS Code remote connection
    Then the required VS Code extensions should be installed
    And VS Code should be configured for remote SSH access
    And I should be able to open remote workspace

@FileSync
Scenario: Setup File Synchronization
    Given I have VS Code remote connection established
    And I have local project files to synchronize
    When I start file synchronization between local and remote
    Then files should be synchronized successfully
    And changes should be monitored in real-time
    And bidirectional sync should be maintained

@Integration
Scenario: Complete End-to-End Automation Workflow
    Given I have all prerequisites met
    When I run the complete automation workflow
    Then the Azure client should be initialized
    And the ML workspace should be connected
    And a compute instance should be created
    And SSH connection should be established
    And VS Code remote should be configured
    And file synchronization should be active
    And the development environment should be ready

@ErrorHandling
Scenario: Handle Authentication Failures
    Given I have invalid Azure credentials
    When I attempt to initialize the Azure client
    Then the authentication should fail gracefully
    And appropriate error messages should be displayed
    And the system should not crash

@ErrorHandling
Scenario: Handle Network Connectivity Issues
    Given I have network connectivity issues
    When I attempt to connect to Azure ML services
    Then the connection should fail gracefully
    And appropriate error messages should be displayed
    And retry mechanisms should be suggested

@ErrorHandling
Scenario: Handle Compute Instance Creation Failures
    Given I have valid authentication but insufficient permissions
    When I attempt to create a compute instance
    Then the creation should fail gracefully
    And appropriate error messages should be displayed
    And permission requirements should be explained

@Cleanup
Scenario: Cleanup Resources After Testing
    Given I have test resources created
    When I run the cleanup process
    Then all test compute instances should be deleted
    And SSH configurations should be cleaned up
    And temporary files should be removed
    And no resources should be left running

@Performance
Scenario: Automation Performance Requirements
    Given I have all prerequisites met
    When I run the automation workflow
    Then the Azure client initialization should complete within 10 seconds
    And the workspace connection should complete within 15 seconds
    And the compute instance creation should complete within 5 minutes
    And the SSH setup should complete within 30 seconds
    And the VS Code setup should complete within 2 minutes

@Security
Scenario: Secure SSH Key Management
    When I generate SSH keys for automation
    Then the private key should have proper permissions (600)
    And the public key should be properly formatted
    And the keys should be stored in the correct location
    And the SSH config should use secure settings

@Security
Scenario: Secure File Transfer
    Given I have file synchronization active
    When files are transferred between local and remote
    Then all transfers should use encrypted protocols (SFTP/SSH)
    And authentication should use SSH keys
    And no credentials should be stored in plain text
    And file permissions should be preserved

@Configuration
Scenario: Configuration Management
    Given I have automation configuration files
    When I load the configuration
    Then all required settings should be present
    And default values should be applied where appropriate
    And configuration validation should pass
    And sensitive information should be properly handled

@Monitoring
Scenario: Monitor Automation Health
    Given I have automation running
    When I check the system health
    Then all connections should be active
    And file synchronization should be working
    And compute instance should be accessible
    And no error conditions should be present