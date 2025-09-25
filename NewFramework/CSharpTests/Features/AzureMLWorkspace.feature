Feature: Azure ML Workspace Management
    As a data scientist
    I want to manage Azure ML workspaces
    So that I can perform machine learning tasks

Background:
    Given I am a data scientist named "Javed"
    And I have activated the Data Scientist PIM role

Scenario: Access Azure ML Workspace
    When I attempt to open workspace "ml-workspace"
    Then I should be able to access the workspace
    And the workspace should be available

Scenario: Start Compute Instance
    Given I have opened workspace "ml-workspace"
    When I start compute instance "test-compute"
    Then the compute instance should be running
    And I should be able to connect to it

Scenario: Stop Compute Instance
    Given I have opened workspace "ml-workspace"
    And compute instance "test-compute" is running
    When I stop compute instance "test-compute"
    Then the compute instance should be stopped

Scenario: Manage Multiple Compute Instances
    Given I have opened workspace "ml-workspace"
    When I start compute instances:
        | ComputeName    |
        | test-compute-1 |
        | test-compute-2 |
    Then all compute instances should be running
    When I stop all compute instances
    Then all compute instances should be stopped

Scenario: Azure ML Workspace with VS Code Desktop Integration
    Given I have Contributor access to Azure ML
    When I go to workspace "https://ml.azure.com/workspaces"
    And If login required I login as user "Javed Khan"
    And I select Workspace "CTO-workspace"
    And I choose compute option
    And I open compute "com-jk"
    And If compute is not running, I start compute
    Then I check if application link are enabled
    When I start VS code Desktop
    Then I check if I am able to interact with VS code