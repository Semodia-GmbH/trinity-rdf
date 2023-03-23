========
Overview
========

Semiodesk Trinity is an application development platform for Microsoft .NET and Mono.
It allows to easily build Linked Data and Semantic Web applications based on the `RDF metadata standard`_ issued by the W3C.
The API allows for developing first-class .NET applications with direct access to Linked Open Data repositories and knowledge graphs.

Our platform is built on top of the powerful and stable `dotNetRDF`_  library which has been in development since early 2009.
Since dotNetRDF is low-level and primarily focused on directly manipulating triples, it does not integrate well with existing application frameworks and introduces a steep learning curve for new developers.
Therefore, our primary goal was to allow developers to use proven enterprise development patterns such as MVC or MVVM to build Linked Data applications that integrate well into existing application eco-systems.

Fork
===========
This fork primarily aims to update Semiodesk Trinity to a modern .NET SDK environment to support all platforms. It also adds custom serializations for Semodia's use case.

License
=======
The library and tools in this repository are all released under the terms of the `MIT license`_. 
This means you can use it for every project you like, even commercial ones, as long as you keep the copyright header intact. 
The source code, documentation and issue tracking can be found at our bitbucket page. 
If you like what we are doing and want to support us, please consider donating.

Dependencies
============
The Semodia.Trinity API has dependencies to 

* `dotNetRDF`_
* `Newtonsoft JSON`_
* `Remotion LINQ`_

The libraries are included in the release package. If you install via NuGet the dependencies should be resolved for you.

Support
=======

.. GENERAL LINKS
.. _`triplestores`: http://en.wikipedia.org/wiki/Triplestore
.. _`MIT license`: http://en.wikipedia.org/wiki/MIT_License
.. _`Semiodesk`: https://www.semiodesk.com
.. _`Semodia`: https://www.semodia.com
.. _`Unity3D`: https://unity3d.com/
.. _`dotNetRDF`: http://dotnetrdf.org/
.. _`OpenLink.Data.Virtuoso`: https://github.com/openlink/virtuoso-opensource
.. _`First Steps`: https://trinity-rdf.net/doc/tutorials/firstSteps.html
.. _`API documentation`: https://trinity-rdf.net/doc/api/
.. _`examples`: https://github.com/semiodesk/trinity-rdf-examples
.. _`RDF metadata standard`: https://w3.org/rdf
.. _`Newtonsoft JSON`: https://www.newtonsoft.com/json
.. _`Remotion LINQ`: https://github.com/re-motion/Relinq