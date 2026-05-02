##########
Unit tests
##########

The MOSA project has an extensive set of unit tests to help validate that the MOSA compiler is emitting correct binary code.

On Windows, execute the script ``Tests\RunAllUnitTestsWithPause.bat`` to run the unit tests.

On Linux, execute the following to run the unit tests:

.. code-block:: bash

	dotnet bin/Mosa.Utility.UnitTests.dll -oMax -check

The unit tests take a few minutes to execute on a modern PC. The results will be automatically displayed on the screen. The last line shows the total number of tests and failed tests, and the total time. Similar to the following:

.. code-block:: text

  Total Elapsed: 95.3 secs

  Unit Test Results:
     Passed:   68164
     Skipped:  4
     Failures: 0
     Total:    68168

  All unit tests passed successfully!

Persistent Bisector
-------------------

Use ``Mosa.Utility.UnitTestBisector.Persistent`` to run bisector plans that can resume after interruption.

.. code-block:: bash

	dotnet bin/Mosa.Utility.UnitTestBisector.Persistent.dll -bisect -bisect-stage <StageTypeName> -bisect-persist-state artifact/bisect-state.json

``-bisect`` is an alias for ``-bisect-plan disable-one``.

Supported plans:

- ``-bisect-plan disable-one``: disable one transform at a time
- ``-bisect-plan enable-one``: enable one transform at a time
- ``-bisect-plan random-combo``: randomly enable/disable all transforms each iteration (persistent, resumable)

Optional ordering for deterministic plans:

- ``-bisect-order original``: discovery order (default)
- ``-bisect-order count``: prioritize lower-observed transforms first
- ``-bisect-order random``: randomized order (seeded via ``-bisect-seed``)

For ``random-combo``, use ``-bisect-iterations <N>`` (default 20) to control how many iterations are run per invocation.

Supervisor
----------

Use ``Mosa.Utility.UnitTestBisector.Supervisor`` to run one bisector worker iteration per child process and automatically restart until completion.

.. code-block:: bash

	dotnet bin/Mosa.Utility.UnitTestBisector.Supervisor.dll -bisect -bisect-stage <StageTypeName> -bisect-persist-state artifact/bisect-state.json -bisect-worker-iteration
