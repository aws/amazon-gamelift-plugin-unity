deploy_scenario_1:
	python3 bin/deploy_cfn.py us-west-2 MyGame src/scenario1_auth_only/cloudformation.yml src/scenario1_auth_only/parameters.json TestDeploymentScenario1Stack src/scenario1_auth_only/lambda/ build/
test_scenario_1:
	python3 tst/scenario1_auth_only/test.py
deploy_scenario_2:
	python3 bin/deploy_cfn.py us-west-2 MyGame src/scenario2_single_fleet/cloudformation.yml src/scenario2_single_fleet/parameters.json TestDeploymentScenario3Stack src/scenario2_single_fleet/lambda/ build/ gaad-us-west-2 GameLiftUnityPluginSample.zip
test_scenario_2:
	python3 tst/scenario2_single_fleet/test.py
deploy_scenario_3:
	python3 bin/deploy_cfn.py us-west-2 MyGame src/scenario3_mrf_queue/cloudformation.yml src/scenario3_mrf_queue/parameters.json TestDeploymentScenario3Stack src/scenario3_mrf_queue/lambda/ build/ gaad-us-west-2 GameLiftUnityPluginSample.zip
test_scenario_3:
	python3 tst/scenario3_mrf_queue/test.py
deploy_scenario_4:
	python3 bin/deploy_cfn.py us-west-2 MyGame src/scenario4_spot_fleets/cloudformation.yml src/scenario4_spot_fleets/parameters.json TestDeploymentScenario4Stack src/scenario4_spot_fleets/lambda/ build/ gaad-us-west-2 GameLiftUnityPluginSample.zip
test_scenario_4:
	python3 tst/scenario4_spot_fleets/test.py
deploy_scenario_5:
	python3 bin/deploy_cfn.py us-west-2 MyGame src/scenario5_flexmatch/cloudformation.yml src/scenario5_flexmatch/parameters.json TestDeploymentScenario5Stack src/scenario5_flexmatch/lambda/ build/ gaad-us-west-2 GameLiftUnityPluginSample.zip
test_scenario_5:
	python3 tst/scenario5_flexmatch/test.py
