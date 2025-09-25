Feature: Azure ML Workspace Management (TypeScript)
  As a data scientist
  I want to manage Azure ML workspace and compute instances
  So that I can perform machine learning tasks efficiently

  Background:
    Given I am a data scientist named "Javed"
    And I have activated my Data Scientist PIM role

  @typescript @azureml
  Scenario: Access Azure ML Workspace
    Given I navigate to the Azure ML workspace
    When I handle authentication if required
    Then I should be able to access the workspace
    And the workspace should be available

  @typescript @azureml @compute
  Scenario: Start Compute Instance
    Given I have access to the Azure ML workspace
    And I navigate to the compute section
    When I start a compute instance named "test-compute"
    Then the compute instance should be in "Running" status
    And the compute instance should be connectable

  @typescript @azureml @compute
  Scenario: Stop Compute Instance
    Given I have access to the Azure ML workspace
    And I navigate to the compute section
    And I ensure compute instance "test-compute" is running
    When I stop the compute instance "test-compute"
    Then the compute instance should be in "Stopped" status

  @typescript @azureml @compute @multiple
  Scenario: Manage Multiple Compute Instances
    Given I have access to the Azure ML workspace
    And I navigate to the compute section
    When I start multiple compute instances:
      | instance_name  |
      | test-compute-1 |
      | test-compute-2 |
    Then all compute instances should be in "Running" status
    When I stop all compute instances
    Then all compute instances should be in "Stopped" status

  @typescript @azureml @vscode @integration
  Scenario: Azure ML Workspace with VS Code Desktop Integration
    Given I navigate to Azure ML workspaces portal
    When I handle login for user "Javed Khan"
    And I select workspace "CTO-workspace"
    And I choose compute option
    And I open compute instance "com-jk"
    And I start compute if not running
    Then application links should be enabled
    When I start VS Code Desktop
    Then VS Code Desktop should be interactive and responsive