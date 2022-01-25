import { expect as expectCDK, matchTemplate, MatchStyle } from '@aws-cdk/assert';
import * as cdk from '@aws-cdk/core';
import * as Gamelift from '../lib/gamelift-stack';

test('Empty Stack', () => {
    const app = new cdk.App();
    // WHEN
    const stack = new Gamelift.GameliftStack(app, 'MyTestStack');
    // THEN
    expectCDK(stack).to(matchTemplate({
      "Resources": {}
    }, MatchStyle.EXACT))
});
