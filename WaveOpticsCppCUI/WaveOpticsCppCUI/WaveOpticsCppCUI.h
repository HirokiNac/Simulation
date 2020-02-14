#pragma once
#include<string>

#include <iostream>
#include <string>
#include <fstream>
#include <sstream>
#include <vector>
using namespace std;

class WaveOpticsCppCUI
{

public:

	vector<vector<double>> GetData(string _fileName);
};

