![WiM](wimlogo.png)


# STN Services 2

Servies for the USGS Short-Term Network (STN)

### Prerequisites

[Visual Studio 2017](https://www.visualstudio.com/)

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Installing

https://help.github.com/articles/cloning-a-repository/

Open the solution file (.sln) using perfered IDE.

## Building and testing

No testing files are currently available for this repository

## Deployment on IIS

* Enable .Net Extensibility 4.7 in "Add/Remove Windows Features":
* ![image](https://user-images.githubusercontent.com/12737515/165164637-97ecabf2-bd1f-4691-91ff-b0fb78427b8d.png)
* Create new application pool specifying the .netCLR version property to 4.0
* Install Npgsql 3.2.5 [link](https://github.com/npgsql/npgsql/releases?page=6)

## Built With

* [.Net Framework](https://docs.microsoft.com/en-us/dotnet/framework/) - ASP.NET Framework

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for submitting pull requests to us. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for details on adhering by the [USGS Code of Scientific Conduct](https://www2.usgs.gov/fsp/fsp_code_of_scientific_conduct.asp).

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](../../tags). 

Advance the version when adding features, fixing bugs or making minor enhancement. Follow semver principles. To add tag in git, type git tag v{major}.{minor}.{patch}. Example: git tag v2.0.5

To push tags to remote origin: `git push origin --tags`

*Note that your alias for the remote origin may differ.

## Authors

* **[Jeremy Newson](https://www.usgs.gov/staff-profiles/jeremy-k-newson)**  - *Developer* - [USGS Web Informatics & Mapping](https://wim.usgs.gov/)
* **[Tonia Roddick](https://github.com/troddick)**  - *Developer*

See also the list of [contributors](../../graphs/contributors) who participated in this project.

## License

This project is licensed under the Creative Commons CC0 1.0 Universal License - see the [LICENSE.md](LICENSE.md) file for details

## Suggested Citation
In the spirit of open source, please cite any re-use of the source code stored in this repository. Below is the suggested citation:

`This project contains code produced by the Web Informatics and Mapping (WIM) team at the United States Geological Survey (USGS). As a work of the United States Government, this project is in the public domain within the United States. https://wim.usgs.gov`


## Acknowledgements

 

## About WIM
* This project authored by the [USGS WIM team](https://wim.usgs.gov)
* WIM is a team of developers and technologists who build and manage tools, software, web services, and databases to support USGS science and other federal government cooperators.
* WIM is a part of the [Upper Midwest Water Science Center](https://www.usgs.gov/centers/wisconsin-water-science-center).
